using Whycespace.Domain.EconomicSystem.Revenue.Distribution;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Revenue.Distribution;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Engines.T2E.Economic.Revenue.Distribution;

/// <summary>
/// Phase 7 B2 / T7.3 — transitions the DistributionAggregate from
/// Paid|Failed to CompensationRequested. The aggregate enforces that
/// <see cref="RequestDistributionCompensationCommand.OriginalPayoutId"/>
/// is non-empty (T7.2 correlation invariant) — this handler forwards it
/// verbatim so cross-aggregate replay always resolves to the same
/// distribution/payout pair.
///
/// Idempotent: aggregate short-circuits via
/// <see cref="DistributionErrors.AlreadyCompensated"/> when Status is
/// already CompensationRequested or Compensated.
/// </summary>
public sealed class RequestDistributionCompensationHandler : IEngine
{
    private readonly IClock _clock;

    public RequestDistributionCompensationHandler(IClock clock) => _clock = clock;

    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RequestDistributionCompensationCommand cmd)
            return;

        var aggregate = (DistributionAggregate)await context.LoadAggregateAsync(typeof(DistributionAggregate));
        aggregate.RequestCompensation(
            cmd.OriginalPayoutId.ToString(),
            cmd.Reason,
            new Timestamp(_clock.UtcNow));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
