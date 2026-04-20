using System.Diagnostics;
using Whycespace.Runtime.Observability;
using Whycespace.Shared.Contracts.Runtime;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Runtime.Dispatcher;

/// <summary>
/// System-level command dispatcher. Auto-generates deterministic IDs and context
/// for system-originated commands. Routes through the RuntimeControlPlane
/// ensuring all middleware (policy, guards, etc.) is applied.
///
/// WP-1 (Security Binding Completion): ActorId and TenantId are now sourced
/// from the authenticated HTTP caller via ICallerIdentityAccessor. The previous
/// hardcoded "system" / "default" values are removed — fail-closed. The accessor
/// throws if no authenticated identity is present on the current request.
/// </summary>
public sealed class SystemIntentDispatcher : ISystemIntentDispatcher
{
    private readonly IRuntimeControlPlane _controlPlane;
    private readonly IIdGenerator _idGenerator;
    private readonly ICallerIdentityAccessor _callerIdentity;
    private readonly ICommandPolicyIdRegistry _policyIdRegistry;

    public SystemIntentDispatcher(
        IRuntimeControlPlane controlPlane,
        IIdGenerator idGenerator,
        ICallerIdentityAccessor callerIdentity,
        ICommandPolicyIdRegistry policyIdRegistry)
    {
        _controlPlane = controlPlane;
        _idGenerator = idGenerator;
        _callerIdentity = callerIdentity;
        _policyIdRegistry = policyIdRegistry;
    }

    // Canonical aggregate-id property names accepted as the reflection
    // fallback. Order matters only for the very rare case of a command record
    // that declares more than one of these — the first match wins, matching
    // the historical "Id" precedence used by Todo/Kanban. Capital commands
    // declare exactly one of {AccountId, …, VaultId}, so collisions are not
    // possible in current contracts.
    private static readonly string[] AggregateIdPropertyCandidates =
    [
        "Id",
        "AccountId",
        "AllocationId",
        "AssetId",
        "BindingId",
        "PoolId",
        "ReserveId",
        "VaultId",
        "LedgerId",
        "JournalId",
        "EntryId",
        "ObligationId",
        "TreasuryId"
    ];

    public Task<CommandResult> DispatchAsync(object command, DomainRoute route, CancellationToken cancellationToken = default) =>
        DispatchInternalAsync(command, route, isSystem: false, cancellationToken);

    /// <summary>
    /// Phase 2.5 — system-origin dispatch. Identical to
    /// <see cref="DispatchAsync"/> except <c>CommandContext.IsSystem</c>
    /// is stamped <c>true</c> so the <c>EnforcementGuard</c> bypass path
    /// is taken for workflow / compensation / recovery commands.
    /// </summary>
    public Task<CommandResult> DispatchSystemAsync(object command, DomainRoute route, CancellationToken cancellationToken = default) =>
        DispatchInternalAsync(command, route, isSystem: true, cancellationToken);

    private async Task<CommandResult> DispatchInternalAsync(
        object command,
        DomainRoute route,
        bool isSystem,
        CancellationToken cancellationToken)
    {
        var commandType = command.GetType();

        // Aggregate-id resolution. Preferred: command opts into IHasAggregateId.
        // Fallback: reflect over the canonical property-name list above. No
        // loose Guid scanning — preserves determinism and prevents accidental
        // binding to an unrelated Guid field.
        var aggregateId = ResolveAggregateId(command, commandType);

        // WP-1: Extract identity from authenticated HTTP caller.
        // Fail-closed — throws if no valid identity exists.
        var actorId = _callerIdentity.GetActorId();
        var tenantId = _callerIdentity.GetTenantId();

        // phase1.6-S1.1 (DET-SEED-DERIVATION-01): correlation/causation/command
        // ids derive ONLY from stable command coordinates — never from the
        // clock. The command's record ToString includes every init property,
        // so two distinct command instances produce distinct ids while two
        // dispatches of the same instance collapse (correct semantics for
        // idempotency / retry under the IdempotencyMiddleware).
        var commandSignature = command.ToString() ?? commandType.Name;
        var correlationId = _idGenerator.Generate($"{aggregateId}:{commandType.Name}:correlation:{commandSignature}");
        var causationId = _idGenerator.Generate($"{aggregateId}:{commandType.Name}:causation:{commandSignature}");
        var commandId = _idGenerator.Generate($"{aggregateId}:{commandType.Name}:command:{commandSignature}");

        var context = new CommandContext
        {
            CorrelationId = correlationId,
            CausationId = causationId,
            CommandId = commandId,
            TenantId = tenantId,
            ActorId = actorId,
            AggregateId = aggregateId,
            // E5.1 — per-command policy id resolved via the registry. Capital
            // commands resolve to their canonical Capital{Domain}PolicyIds value;
            // unmapped commands fall back to "whyce-policy-default" so existing
            // flows (Todo, Kanban, etc.) continue unchanged until they bind.
            PolicyId = _policyIdRegistry.Resolve(commandType),
            Classification = route.Classification,
            Context = route.Context,
            Domain = route.Domain,
            // Phase 2.5 — stamped here and only here. The init-only property
            // ensures no downstream middleware, handler, or API path can
            // promote a user command to system after construction.
            IsSystem = isSystem
        };

        // R5.A / R-TRACE-DISPATCH-SPAN-01 — wrap the control-plane execution
        // in a canonical runtime.command.dispatch span. The span carries the
        // full routing-coordinate set plus actor/correlation ids so operators
        // can filter in Jaeger on any canonical dimension. Span name is
        // FIXED (low-cardinality); command type lives on an attribute.
        using var activity = WhyceActivitySources.ControlPlane.StartActivity(
            WhyceActivitySources.Spans.CommandDispatch,
            ActivityKind.Internal);
        activity?.SetTag(WhyceActivitySources.Attributes.CommandType, commandType.Name);
        activity?.SetTag(WhyceActivitySources.Attributes.Classification, route.Classification);
        activity?.SetTag(WhyceActivitySources.Attributes.Context, route.Context);
        activity?.SetTag(WhyceActivitySources.Attributes.Domain, route.Domain);
        activity?.SetTag(WhyceActivitySources.Attributes.AggregateId, aggregateId);
        activity?.SetTag(WhyceActivitySources.Attributes.ActorId, actorId);
        activity?.SetTag(WhyceActivitySources.Attributes.TenantId, tenantId);
        activity?.SetTag(WhyceActivitySources.Attributes.CorrelationId, correlationId);

        // phase1.5-S5.2.3 / TC-1 (DISPATCHER-CT-CONTRACT-01): forward
        // the cancellation token into the control plane so the locked
        // middleware pipeline can honor request/shutdown cancellation.
        CommandResult result;
        try
        {
            result = await _controlPlane.ExecuteAsync(command, context, cancellationToken);
        }
        catch (Exception ex)
        {
            // R5.A — canonical failure recording on the span. The exception
            // itself re-propagates untouched (layer discipline: the
            // dispatcher NEVER swallows) so the HTTP edge / canonical
            // exception handlers still see the typed failure.
            activity?.SetStatus(ActivityStatusCode.Error, ex.GetType().Name);
            activity?.SetTag(WhyceActivitySources.Attributes.Outcome, "exception");
            activity?.SetTag(WhyceActivitySources.Attributes.FailureReason, ex.GetType().Name);
            throw;
        }

        activity?.SetStatus(result.IsSuccess ? ActivityStatusCode.Ok : ActivityStatusCode.Error);
        activity?.SetTag(WhyceActivitySources.Attributes.Outcome, result.IsSuccess ? "success" : "failure");
        if (!result.IsSuccess && !string.IsNullOrEmpty(result.Error))
            activity?.SetTag(WhyceActivitySources.Attributes.FailureReason, result.Error);

        // phase1-gate-S7: stamp the correlation id used by EventStore /
        // WhyceChain / Outbox onto the response so the API caller can trace
        // a single request through every persistence boundary.
        return result with { CorrelationId = correlationId };
    }

    private static Guid ResolveAggregateId(object command, Type commandType)
    {
        // 1. Explicit interface — preferred.
        if (command is IHasAggregateId typed)
            return typed.AggregateId;

        // 2. Canonical property-name list — covers Todo/Kanban (Id) and
        //    capital commands (AccountId, AllocationId, …).
        foreach (var name in AggregateIdPropertyCandidates)
        {
            var prop = commandType.GetProperty(name);
            if (prop is not null && prop.PropertyType == typeof(Guid))
                return (Guid)prop.GetValue(command)!;
        }

        // 3. No match — fail loud rather than silently dispatch with
        //    Guid.Empty. The caller has wired a command type the dispatcher
        //    cannot route; surfacing it immediately is the right answer.
        throw new InvalidOperationException(
            $"SystemIntentDispatcher cannot resolve an aggregate id for command type '{commandType.FullName}'. " +
            $"Implement IHasAggregateId, or expose a Guid property named one of: " +
            $"{string.Join(", ", AggregateIdPropertyCandidates)}.");
    }
}
