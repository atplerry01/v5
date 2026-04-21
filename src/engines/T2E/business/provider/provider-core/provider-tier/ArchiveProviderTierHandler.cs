using Whycespace.Domain.BusinessSystem.Provider.ProviderCore.ProviderTier;
using Whycespace.Shared.Contracts.Business.Provider.ProviderCore.ProviderTier;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Provider.ProviderCore.ProviderTier;

public sealed class ArchiveProviderTierHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ArchiveProviderTierCommand)
            return;

        var aggregate = (ProviderTierAggregate)await context.LoadAggregateAsync(typeof(ProviderTierAggregate));
        aggregate.Archive();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
