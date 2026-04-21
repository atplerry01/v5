using Whycespace.Domain.BusinessSystem.Offering.CatalogCore.Bundle;
using Whycespace.Shared.Contracts.Business.Offering.CatalogCore.Bundle;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Offering.CatalogCore.Bundle;

public sealed class ArchiveBundleHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ArchiveBundleCommand)
            return;

        var aggregate = (BundleAggregate)await context.LoadAggregateAsync(typeof(BundleAggregate));
        aggregate.Archive();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
