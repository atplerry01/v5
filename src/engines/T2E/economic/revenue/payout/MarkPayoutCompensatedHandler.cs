using Whycespace.Domain.EconomicSystem.Revenue.Payout;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Revenue.Payout;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Engines.T2E.Economic.Revenue.Payout;

/// <summary>
/// Phase 7 B2 / T7.3 — terminal payout transition CompensationRequested
/// → Compensated. Requires the compensating journal id posted by
/// PostCompensatingLedgerJournalStep (T7.4) so the reversal correlation
/// is sealed onto the aggregate's event stream.
///
/// Idempotent: the aggregate throws <see cref="PayoutErrors.AlreadyCompensated"/>
/// when Status == Compensated, so replays do not re-emit
/// <c>PayoutCompensatedEvent</c>. Compensated is terminal and irreversible
/// (T7.2).
/// </summary>
public sealed class MarkPayoutCompensatedHandler : IEngine
{
    private readonly IClock _clock;

    public MarkPayoutCompensatedHandler(IClock clock) => _clock = clock;

    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not MarkPayoutCompensatedCommand cmd)
            return;

        var aggregate = (PayoutAggregate)await context.LoadAggregateAsync(typeof(PayoutAggregate));
        aggregate.MarkCompensated(cmd.CompensatingJournalId, new Timestamp(_clock.UtcNow));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
