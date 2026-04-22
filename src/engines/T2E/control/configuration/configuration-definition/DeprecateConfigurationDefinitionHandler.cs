using Whycespace.Domain.ControlSystem.Configuration.ConfigurationDefinition;
using Whycespace.Shared.Contracts.Control.Configuration.ConfigurationDefinition;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Control.Configuration.ConfigurationDefinition;

public sealed class DeprecateConfigurationDefinitionHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not DeprecateConfigurationDefinitionCommand)
            return;

        var aggregate = (ConfigurationDefinitionAggregate)await context.LoadAggregateAsync(typeof(ConfigurationDefinitionAggregate));
        aggregate.Deprecate();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
