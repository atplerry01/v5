using Whycespace.Domain.BusinessSystem.Provider.ProviderScope.ProviderAvailability;
using Whycespace.Domain.BusinessSystem.Shared.Time;
using Whycespace.Domain.StructuralSystem.Contracts.References;
using Whycespace.Shared.Contracts.Business.Provider.ProviderScope.ProviderAvailability;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Business.Provider.ProviderScope.ProviderAvailability;

public sealed class CreateProviderAvailabilityHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateProviderAvailabilityCommand cmd)
            return Task.CompletedTask;

        var aggregate = ProviderAvailabilityAggregate.Create(
            new ProviderAvailabilityId(cmd.ProviderAvailabilityId),
            new ClusterProviderRef(cmd.ProviderId),
            new TimeWindow(cmd.StartsAt, cmd.EndsAt));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
