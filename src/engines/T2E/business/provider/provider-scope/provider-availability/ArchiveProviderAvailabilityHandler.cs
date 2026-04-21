using Whycespace.Domain.BusinessSystem.Provider.ProviderScope.ProviderAvailability;
using Whycespace.Shared.Contracts.Business.Provider.ProviderScope.ProviderAvailability;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Provider.ProviderScope.ProviderAvailability;

public sealed class ArchiveProviderAvailabilityHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ArchiveProviderAvailabilityCommand)
            return;

        var aggregate = (ProviderAvailabilityAggregate)await context.LoadAggregateAsync(typeof(ProviderAvailabilityAggregate));
        aggregate.Archive();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
