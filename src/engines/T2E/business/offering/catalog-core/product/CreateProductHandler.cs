using Whycespace.Domain.BusinessSystem.Offering.CatalogCore.Product;
using Whycespace.Shared.Contracts.Business.Offering.CatalogCore.Product;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Offering.CatalogCore.Product;

public sealed class CreateProductHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateProductCommand cmd)
            return Task.CompletedTask;

        // ProductType is transported as the enum name so the wire schema stays
        // decoupled from the ProductType CLR type. The domain value-object guard
        // will re-validate; an unknown name fails loudly.
        var type = Enum.Parse<ProductType>(cmd.Type, ignoreCase: false);
        CatalogRef? catalog = cmd.CatalogId is { } cid
            ? new CatalogRef(cid)
            : null;

        var aggregate = ProductAggregate.Create(
            new ProductId(cmd.ProductId),
            new ProductName(cmd.Name),
            type,
            catalog);
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
