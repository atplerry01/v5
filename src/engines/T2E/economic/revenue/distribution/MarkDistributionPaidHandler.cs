using Whycespace.Domain.EconomicSystem.Revenue.Distribution;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Revenue.Distribution;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Engines.T2E.Economic.Revenue.Distribution;

public sealed class MarkDistributionPaidHandler : IEngine
{
    private readonly IClock _clock;

    public MarkDistributionPaidHandler(IClock clock) => _clock = clock;

    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not MarkDistributionPaidCommand cmd)
            return;

        var aggregate = (DistributionAggregate)await context.LoadAggregateAsync(typeof(DistributionAggregate));
        aggregate.MarkPaid(cmd.PayoutId.ToString(), new Timestamp(_clock.UtcNow));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
