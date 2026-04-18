using Whycespace.Domain.EconomicSystem.Revenue.Distribution;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Revenue.Distribution;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Engines.T2E.Economic.Revenue.Distribution;

/// <summary>
/// Phase 7 B2 / T7.3 — terminal distribution transition
/// CompensationRequested → Compensated. Binds the sibling payout id and
/// the compensating journal id produced by PayoutCompensationWorkflow
/// onto the distribution stream, sealing the cross-aggregate reversal
/// correlation.
///
/// Idempotent: the aggregate rejects duplicate dispatches after Compensated
/// via <see cref="DistributionErrors.AlreadyCompensated"/> and rejects
/// an out-of-order call before CompensationRequested via
/// <see cref="DistributionErrors.CompensationNotRequested"/>.
/// </summary>
public sealed class MarkDistributionCompensatedHandler : IEngine
{
    private readonly IClock _clock;

    public MarkDistributionCompensatedHandler(IClock clock) => _clock = clock;

    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not MarkDistributionCompensatedCommand cmd)
            return;

        var aggregate = (DistributionAggregate)await context.LoadAggregateAsync(typeof(DistributionAggregate));
        aggregate.MarkCompensated(
            cmd.OriginalPayoutId.ToString(),
            cmd.CompensatingJournalId,
            new Timestamp(_clock.UtcNow));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
