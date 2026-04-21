using Whycespace.Domain.BusinessSystem.Provider.ProviderCore.ProviderCapability;
using Whycespace.Shared.Contracts.Business.Provider.ProviderCore.ProviderCapability;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Provider.ProviderCore.ProviderCapability;

public sealed class ArchiveProviderCapabilityHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ArchiveProviderCapabilityCommand)
            return;

        var aggregate = (ProviderCapabilityAggregate)await context.LoadAggregateAsync(typeof(ProviderCapabilityAggregate));
        aggregate.Archive();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
