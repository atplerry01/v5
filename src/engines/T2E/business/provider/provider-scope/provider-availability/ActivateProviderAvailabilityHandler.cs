using Whycespace.Domain.BusinessSystem.Provider.ProviderScope.ProviderAvailability;
using Whycespace.Shared.Contracts.Business.Provider.ProviderScope.ProviderAvailability;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Provider.ProviderScope.ProviderAvailability;

public sealed class ActivateProviderAvailabilityHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ActivateProviderAvailabilityCommand)
            return;

        var aggregate = (ProviderAvailabilityAggregate)await context.LoadAggregateAsync(typeof(ProviderAvailabilityAggregate));
        aggregate.Activate();
        context.EmitEvents(aggregate.DomainEvents);
    }
}
