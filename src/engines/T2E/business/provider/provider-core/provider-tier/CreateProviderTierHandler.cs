using Whycespace.Domain.BusinessSystem.Provider.ProviderCore.ProviderTier;
using Whycespace.Shared.Contracts.Business.Provider.ProviderCore.ProviderTier;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Provider.ProviderCore.ProviderTier;

public sealed class CreateProviderTierHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateProviderTierCommand cmd)
            return Task.CompletedTask;

        var aggregate = ProviderTierAggregate.Create(
            new ProviderTierId(cmd.ProviderTierId),
            new TierCode(cmd.Code),
            new TierName(cmd.Name),
            new TierRank(cmd.Rank));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
