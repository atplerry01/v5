using Whycespace.Domain.EconomicSystem.Revenue.Distribution;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Revenue.Distribution;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Engines.T2E.Economic.Revenue.Distribution;

public sealed class ConfirmDistributionHandler : IEngine
{
    private readonly IClock _clock;

    public ConfirmDistributionHandler(IClock clock) => _clock = clock;

    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ConfirmDistributionCommand)
            return;

        var aggregate = (DistributionAggregate)await context.LoadAggregateAsync(typeof(DistributionAggregate));
        aggregate.Confirm(new Timestamp(_clock.UtcNow));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
