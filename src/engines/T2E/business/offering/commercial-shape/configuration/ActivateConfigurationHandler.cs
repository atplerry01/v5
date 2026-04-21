using Whycespace.Domain.BusinessSystem.Offering.CommercialShape.Configuration;
using Whycespace.Shared.Contracts.Business.Offering.CommercialShape.Configuration;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Offering.CommercialShape.Configuration;

public sealed class ActivateConfigurationHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ActivateConfigurationCommand)
            return;

        var aggregate = (ConfigurationAggregate)await context.LoadAggregateAsync(typeof(ConfigurationAggregate));
        aggregate.Activate();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
