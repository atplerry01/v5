using Whycespace.Domain.BusinessSystem.Offering.CatalogCore.Bundle;
using Whycespace.Shared.Contracts.Business.Offering.CatalogCore.Bundle;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Offering.CatalogCore.Bundle;

public sealed class CreateBundleHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateBundleCommand cmd)
            return Task.CompletedTask;

        var aggregate = BundleAggregate.Create(
            new BundleId(cmd.BundleId),
            new BundleName(cmd.Name));
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
