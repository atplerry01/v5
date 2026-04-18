using Whycespace.Domain.EconomicSystem.Revenue.Distribution;
using Whycespace.Domain.EconomicSystem.Revenue.Payout;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.EconomicSystem.Revenue.Payout;

public sealed class PayoutTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp Ts = new(DateTime.SpecifyKind(new DateTime(2026, 4, 17), DateTimeKind.Utc));

    [Fact]
    public void Request_FromDistributionShares_RaisesPayoutRequestedEventInRequestedState()
    {
        var payoutId = new PayoutId(IdGen.Generate("PayoutTests:Request:payout"));
        var distributionId = new DistributionId(IdGen.Generate("PayoutTests:Request:distribution"));
        var idempotencyKey = new PayoutIdempotencyKey($"payout|{distributionId.Value:N}|spv-1");

        var shares = new List<ParticipantShare>
        {
            new("participant-a", 600m, 60m),
            new("participant-b", 400m, 40m),
        };

        var aggregate = PayoutAggregate.Request(payoutId, distributionId, idempotencyKey, shares, Ts);

        var evt = Assert.IsType<PayoutRequestedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(payoutId.Value.ToString(), evt.PayoutId);
        Assert.Equal(distributionId.Value.ToString(), evt.DistributionId);
        Assert.Equal(idempotencyKey.Value, evt.IdempotencyKey);
    }

    [Fact]
    public void MarkExecuted_FromRequested_RaisesPayoutExecutedEventAndTransitionsToExecuted()
    {
        var aggregate = NewRequested(out var payoutId, out var distributionId, out var idempotencyKey);
        aggregate.ClearDomainEvents();

        aggregate.MarkExecuted(Ts);

        var evt = Assert.IsType<PayoutExecutedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(payoutId.Value.ToString(), evt.PayoutId);
        Assert.Equal(distributionId.Value.ToString(), evt.DistributionId);
        Assert.Equal(idempotencyKey.Value, evt.IdempotencyKey);
    }

    [Fact]
    public void MarkExecuted_WhenAlreadyExecuted_IsIdempotent()
    {
        var aggregate = NewRequested(out _, out _, out _);
        aggregate.MarkExecuted(Ts);
        aggregate.ClearDomainEvents();

        aggregate.MarkExecuted(Ts);

        Assert.Empty(aggregate.DomainEvents);
    }

    [Fact]
    public void MarkFailed_FromRequested_RaisesPayoutFailedEvent()
    {
        var aggregate = NewRequested(out _, out _, out _);
        aggregate.ClearDomainEvents();

        aggregate.MarkFailed("conservation violated", Ts);

        var evt = Assert.IsType<PayoutFailedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal("conservation violated", evt.Reason);
    }

    private static PayoutAggregate NewRequested(
        out PayoutId payoutId,
        out DistributionId distributionId,
        out PayoutIdempotencyKey idempotencyKey)
    {
        payoutId = new PayoutId(IdGen.Generate("PayoutTests:Helper:payout"));
        distributionId = new DistributionId(IdGen.Generate("PayoutTests:Helper:distribution"));
        idempotencyKey = new PayoutIdempotencyKey($"payout|{distributionId.Value:N}|spv-helper");

        var shares = new List<ParticipantShare>
        {
            new("participant-a", 600m, 60m),
            new("participant-b", 400m, 40m),
        };

        return PayoutAggregate.Request(payoutId, distributionId, idempotencyKey, shares, Ts);
    }
}
