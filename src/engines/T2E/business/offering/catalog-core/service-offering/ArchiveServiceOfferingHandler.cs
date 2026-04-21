using Whycespace.Domain.BusinessSystem.Offering.CatalogCore.ServiceOffering;
using Whycespace.Shared.Contracts.Business.Offering.CatalogCore.ServiceOffering;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Offering.CatalogCore.ServiceOffering;

public sealed class ArchiveServiceOfferingHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ArchiveServiceOfferingCommand)
            return;

        var aggregate = (ServiceOfferingAggregate)await context.LoadAggregateAsync(typeof(ServiceOfferingAggregate));
        aggregate.Archive();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
