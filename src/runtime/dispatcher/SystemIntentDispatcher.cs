using Whyce.Shared.Contracts.Runtime;
using Whyce.Shared.Kernel.Domain;

namespace Whyce.Runtime.Dispatcher;

/// <summary>
/// System-level command dispatcher. Auto-generates deterministic IDs and context
/// for system-originated commands. Routes through the RuntimeControlPlane
/// ensuring all middleware (policy, guards, etc.) is applied.
/// </summary>
public sealed class SystemIntentDispatcher : ISystemIntentDispatcher
{
    private readonly IRuntimeControlPlane _controlPlane;
    private readonly IIdGenerator _idGenerator;

    public SystemIntentDispatcher(IRuntimeControlPlane controlPlane, IIdGenerator idGenerator)
    {
        _controlPlane = controlPlane;
        _idGenerator = idGenerator;
    }

    public async Task<CommandResult> DispatchAsync(object command, DomainRoute route)
    {
        var commandType = command.GetType();

        // Extract aggregate ID from command by convention (Id property)
        var idProperty = commandType.GetProperty("Id");
        var aggregateId = idProperty is not null ? (Guid)idProperty.GetValue(command)! : Guid.Empty;

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
            TenantId = "default",
            ActorId = "system",
            AggregateId = aggregateId,
            PolicyId = "whyce-policy-default",
            Classification = route.Classification,
            Context = route.Context,
            Domain = route.Domain
        };

        var result = await _controlPlane.ExecuteAsync(command, context);

        // phase1-gate-S7: stamp the correlation id used by EventStore /
        // WhyceChain / Outbox onto the response so the API caller can trace
        // a single request through every persistence boundary.
        return result with { CorrelationId = correlationId };
    }
}
