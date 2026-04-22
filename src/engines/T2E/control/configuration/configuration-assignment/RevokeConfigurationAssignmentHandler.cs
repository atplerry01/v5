using Whycespace.Domain.ControlSystem.Configuration.ConfigurationAssignment;
using Whycespace.Shared.Contracts.Control.Configuration.ConfigurationAssignment;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.Configuration.ConfigurationAssignment;

public sealed class RevokeConfigurationAssignmentHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RevokeConfigurationAssignmentCommand)
            return;

        var aggregate = (ConfigurationAssignmentAggregate)await context.LoadAggregateAsync(typeof(ConfigurationAssignmentAggregate));
        aggregate.Revoke();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
