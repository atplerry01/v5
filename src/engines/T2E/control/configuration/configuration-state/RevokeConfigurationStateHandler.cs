using Whycespace.Domain.ControlSystem.Configuration.ConfigurationState;
using Whycespace.Shared.Contracts.Control.Configuration.ConfigurationState;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.Configuration.ConfigurationState;

public sealed class RevokeConfigurationStateHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RevokeConfigurationStateCommand)
            return;

        var aggregate = (ConfigurationStateAggregate)await context.LoadAggregateAsync(typeof(ConfigurationStateAggregate));
        aggregate.Revoke();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
