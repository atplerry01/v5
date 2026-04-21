using Whycespace.Domain.BusinessSystem.Offering.CatalogCore.Product;
using Whycespace.Shared.Contracts.Business.Offering.CatalogCore.Product;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Offering.CatalogCore.Product;

public sealed class ActivateProductHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ActivateProductCommand)
            return;

        var aggregate = (ProductAggregate)await context.LoadAggregateAsync(typeof(ProductAggregate));
        aggregate.Activate();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
