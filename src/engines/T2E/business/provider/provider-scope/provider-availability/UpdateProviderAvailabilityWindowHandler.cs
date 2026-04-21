using Whycespace.Domain.BusinessSystem.Provider.ProviderScope.ProviderAvailability;
using Whycespace.Domain.BusinessSystem.Shared.Time;
using Whycespace.Shared.Contracts.Business.Provider.ProviderScope.ProviderAvailability;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Provider.ProviderScope.ProviderAvailability;

public sealed class UpdateProviderAvailabilityWindowHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not UpdateProviderAvailabilityWindowCommand cmd)
            return;

        var aggregate = (ProviderAvailabilityAggregate)await context.LoadAggregateAsync(typeof(ProviderAvailabilityAggregate));
        aggregate.UpdateWindow(new TimeWindow(cmd.StartsAt, cmd.EndsAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
