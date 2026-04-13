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

    public SystemIntentDispatcher(
        IRuntimeControlPlane controlPlane,
        IIdGenerator idGenerator,
        ICallerIdentityAccessor callerIdentity)
    {
        _controlPlane = controlPlane;
        _idGenerator = idGenerator;
        _callerIdentity = callerIdentity;
    }

    public async Task<CommandResult> DispatchAsync(object command, DomainRoute route, CancellationToken cancellationToken = default)
    {
        var commandType = command.GetType();

        // Extract aggregate ID from command by convention (Id property)
        var idProperty = commandType.GetProperty("Id");
        var aggregateId = idProperty is not null ? (Guid)idProperty.GetValue(command)! : Guid.Empty;

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
            PolicyId = "whyce-policy-default",
            Classification = route.Classification,
            Context = route.Context,
            Domain = route.Domain
        };

        // phase1.5-S5.2.3 / TC-1 (DISPATCHER-CT-CONTRACT-01): forward
        // the cancellation token into the control plane so the locked
        // middleware pipeline can honor request/shutdown cancellation.
        var result = await _controlPlane.ExecuteAsync(command, context, cancellationToken);

        // phase1-gate-S7: stamp the correlation id used by EventStore /
        // WhyceChain / Outbox onto the response so the API caller can trace
        // a single request through every persistence boundary.
        return result with { CorrelationId = correlationId };
    }
}
