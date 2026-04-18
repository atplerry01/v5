using Whycespace.Domain.EconomicSystem.Revenue.Payout;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Revenue.Payout;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Engines.T2E.Economic.Revenue.Payout;

/// <summary>
/// Phase 7 B2 / T7.3 — transitions the PayoutAggregate from
/// Executed|Failed to CompensationRequested. Idempotent: the aggregate
/// itself short-circuits <c>RequestCompensation</c> when already in
/// CompensationRequested or Compensated (via
/// <see cref="PayoutErrors.AlreadyCompensated"/>), so replayed dispatches
/// never produce duplicate <c>PayoutCompensationRequestedEvent</c>s.
///
/// No new aggregate is created here; the handler loads the existing
/// payout by <see cref="ExecutePayoutCommand.PayoutId"/>, which also
/// stamps the expected version into the event-fabric so the append is
/// concurrency-guarded.
/// </summary>
public sealed class RequestPayoutCompensationHandler : IEngine
{
    private readonly IClock _clock;

    public RequestPayoutCompensationHandler(IClock clock) => _clock = clock;

    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RequestPayoutCompensationCommand cmd)
            return;

        var aggregate = (PayoutAggregate)await context.LoadAggregateAsync(typeof(PayoutAggregate));
        aggregate.RequestCompensation(cmd.Reason, new Timestamp(_clock.UtcNow));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
