using Whycespace.Domain.BusinessSystem.Offering.CatalogCore.Catalog;
using Whycespace.Shared.Contracts.Business.Offering.CatalogCore.Catalog;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Offering.CatalogCore.Catalog;

public sealed class CreateCatalogHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateCatalogCommand cmd)
            return Task.CompletedTask;

        var aggregate = CatalogAggregate.Create(
            new CatalogId(cmd.CatalogId),
            new CatalogStructure(cmd.Name, cmd.Category));
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
