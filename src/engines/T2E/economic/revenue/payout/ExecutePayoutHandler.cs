using Whycespace.Domain.EconomicSystem.Revenue.Distribution;
using Whycespace.Domain.EconomicSystem.Revenue.Payout;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Economic.Revenue.Payout;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Infrastructure.Persistence;
using Whycespace.Shared.Kernel.Domain;

namespace Whycespace.Engines.T2E.Economic.Revenue.Payout;

/// <summary>
/// Phase 7 B2 / T7.5 — opens a payout attempt, gated by
/// <see cref="PayoutRetrySpecification.CanRetry"/>.
///
/// Because <c>PayoutId</c> is deterministic from (DistributionId, SpvId),
/// a retry for the same pair lands on the same aggregate stream. The
/// handler loads prior events directly (mirroring the
/// DOM-LIFECYCLE-INIT-IDEMPOTENT-01.b pattern from
/// <c>RecordRevenueHandler</c>) to distinguish three cases without
/// relying on exception-based control flow:
///
///   1. No prior events       → first attempt, emit PayoutRequested.
///   2. Prior attempt, retryable terminal state
///      (Compensated | Failed) → emit PayoutRequested on top of the
///      existing stream; the aggregate re-enters the Requested state.
///   3. Prior attempt, any other state
///      (Requested | Executed | CompensationRequested) → reject with
///      <see cref="PayoutErrors.RetryRequiresCompensatedOrFailed"/>.
///      Retrying under these states would double-pay: a Requested or
///      Executed prior still has live ledger side-effects, and a
///      CompensationRequested prior is mid-reversal.
/// </summary>
public sealed class ExecutePayoutHandler : IEngine
{
    private readonly IClock _clock;
    private readonly IEventStore _eventStore;

    public ExecutePayoutHandler(IClock clock, IEventStore eventStore)
    {
        _clock = clock;
        _eventStore = eventStore;
    }

    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ExecutePayoutCommand cmd)
            return;

        var prior = await _eventStore.LoadEventsAsync(context.AggregateId);
        if (prior.Count > 0)
        {
            var priorAggregate = (PayoutAggregate)System.Activator.CreateInstance(
                typeof(PayoutAggregate), nonPublic: true)!;
            priorAggregate.HydrateIdentity(context.AggregateId);
            priorAggregate.LoadFromHistory(prior);

            if (!PayoutRetrySpecification.CanRetry(priorAggregate.Status))
                throw PayoutErrors.RetryRequiresCompensatedOrFailed(priorAggregate.Status);
        }

        var shares = new List<ParticipantShare>(cmd.Shares.Count);
        foreach (var s in cmd.Shares)
            shares.Add(new ParticipantShare(s.ParticipantId, s.Amount, s.Percentage));

        var aggregate = PayoutAggregate.Request(
            new PayoutId(cmd.PayoutId),
            new DistributionId(cmd.DistributionId),
            new PayoutIdempotencyKey(cmd.IdempotencyKey),
            shares,
            new Timestamp(_clock.UtcNow));

        context.EmitEvents(aggregate.DomainEvents);
    }
}
