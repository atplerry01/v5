using Whyce.Shared.Contracts.Runtime;
using Whyce.Shared.Kernel.Domain;

namespace Whyce.Runtime.Pipeline;

public sealed class SystemIntentDispatcher : ISystemIntentDispatcher
{
    private readonly ICommandDispatcher _commandDispatcher;
    private readonly IClock _clock;

    public SystemIntentDispatcher(ICommandDispatcher commandDispatcher, IClock clock)
    {
        _commandDispatcher = commandDispatcher;
        _clock = clock;
    }

    public async Task<CommandResult> DispatchAsync(object command)
    {
        // Extract aggregate ID from command by convention (Id property)
        var idProperty = command.GetType().GetProperty("Id");
        var aggregateId = idProperty is not null ? (Guid)idProperty.GetValue(command)! : Guid.Empty;

        var context = new CommandContext
        {
            CorrelationId = Guid.NewGuid(), // TODO: replace with IIdGenerator for determinism
            CausationId = Guid.NewGuid(),
            TenantId = "default",
            ActorId = "system",
            AggregateId = aggregateId,
            PolicyId = "whyce-policy-default"
        };

        return await _commandDispatcher.DispatchAsync(command, context);
    }
}
