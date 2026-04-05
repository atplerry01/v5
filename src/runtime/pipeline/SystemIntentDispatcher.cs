using Whyce.Shared.Contracts.Runtime;
using Whyce.Shared.Kernel.Domain;

namespace Whyce.Runtime.Pipeline;

public sealed class SystemIntentDispatcher : ISystemIntentDispatcher
{
    private readonly ICommandDispatcher _commandDispatcher;
    private readonly IClock _clock;
    private readonly IIdGenerator _idGenerator;

    public SystemIntentDispatcher(ICommandDispatcher commandDispatcher, IClock clock, IIdGenerator idGenerator)
    {
        _commandDispatcher = commandDispatcher;
        _clock = clock;
        _idGenerator = idGenerator;
    }

    public async Task<CommandResult> DispatchAsync(object command)
    {
        var commandType = command.GetType();

        // Extract aggregate ID from command by convention (Id property)
        var idProperty = commandType.GetProperty("Id");
        var aggregateId = idProperty is not null ? (Guid)idProperty.GetValue(command)! : Guid.Empty;

        var timestamp = _clock.UtcNow.Ticks.ToString();
        var correlationId = _idGenerator.Generate($"{aggregateId}:{commandType.Name}:correlation:{timestamp}");
        var causationId = _idGenerator.Generate($"{aggregateId}:{commandType.Name}:causation:{timestamp}");

        var context = new CommandContext
        {
            CorrelationId = correlationId,
            CausationId = causationId,
            TenantId = "default",
            ActorId = "system",
            AggregateId = aggregateId,
            PolicyId = "whyce-policy-default"
        };

        return await _commandDispatcher.DispatchAsync(command, context);
    }
}
