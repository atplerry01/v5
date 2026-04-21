using Whycespace.Domain.BusinessSystem.Offering.CatalogCore.Catalog;
using Whycespace.Shared.Contracts.Business.Offering.CatalogCore.Catalog;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Offering.CatalogCore.Catalog;

public sealed class PublishCatalogHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not PublishCatalogCommand)
            return;

        var aggregate = (CatalogAggregate)await context.LoadAggregateAsync(typeof(CatalogAggregate));
        aggregate.Publish();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
