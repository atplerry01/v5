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
    private readonly IClock _clock;
    private readonly IIdGenerator _idGenerator;

    public SystemIntentDispatcher(IRuntimeControlPlane controlPlane, IClock clock, IIdGenerator idGenerator)
    {
        _controlPlane = controlPlane;
        _clock = clock;
        _idGenerator = idGenerator;
    }

    public async Task<CommandResult> DispatchAsync(object command, DomainRoute route)
    {
        var commandType = command.GetType();

        // Extract aggregate ID from command by convention (Id property)
        var idProperty = commandType.GetProperty("Id");
        var aggregateId = idProperty is not null ? (Guid)idProperty.GetValue(command)! : Guid.Empty;

        var timestamp = _clock.UtcNow.Ticks.ToString();
        var correlationId = _idGenerator.Generate($"{aggregateId}:{commandType.Name}:correlation:{timestamp}");
        var causationId = _idGenerator.Generate($"{aggregateId}:{commandType.Name}:causation:{timestamp}");
        var commandId = _idGenerator.Generate($"{aggregateId}:{commandType.Name}:command:{timestamp}");

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
