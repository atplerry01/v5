using Whycespace.Domain.BusinessSystem.Offering.CatalogCore.Product;
using Whycespace.Shared.Contracts.Business.Offering.CatalogCore.Product;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Offering.CatalogCore.Product;

public sealed class UpdateProductHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not UpdateProductCommand cmd)
            return;

        var aggregate = (ProductAggregate)await context.LoadAggregateAsync(typeof(ProductAggregate));
        var type = Enum.Parse<ProductType>(cmd.Type, ignoreCase: false);
        aggregate.Update(new ProductName(cmd.Name), type);
        context.EmitEvents(aggregate.DomainEvents);
    }
}
