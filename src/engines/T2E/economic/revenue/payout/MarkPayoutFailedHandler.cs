using Whycespace.Domain.EconomicSystem.Revenue.Payout;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Revenue.Payout;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Engines.T2E.Economic.Revenue.Payout;

public sealed class MarkPayoutFailedHandler : IEngine
{
    private readonly IClock _clock;

    public MarkPayoutFailedHandler(IClock clock) => _clock = clock;

    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not MarkPayoutFailedCommand cmd)
            return;

        var aggregate = (PayoutAggregate)await context.LoadAggregateAsync(typeof(PayoutAggregate));
        aggregate.MarkFailed(cmd.Reason, new Timestamp(_clock.UtcNow));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
