using Whycespace.Domain.ControlSystem.Configuration.ConfigurationScope;
using Whycespace.Shared.Contracts.Control.Configuration.ConfigurationScope;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.Configuration.ConfigurationScope;

public sealed class RemoveConfigurationScopeHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RemoveConfigurationScopeCommand)
            return;

        var aggregate = (ConfigurationScopeAggregate)await context.LoadAggregateAsync(typeof(ConfigurationScopeAggregate));
        aggregate.Remove();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
