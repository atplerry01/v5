using Whycespace.Projections.Economic.Revenue.Payout.Reducer;
using Whycespace.Shared.Contracts.Economic.Revenue.Payout;
using Whycespace.Shared.Contracts.Events.Economic.Revenue.Payout;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.EconomicSystem.Revenue.Payout;

/// <summary>
/// Phase 3.5 T3.5.5 — reducer-side projection tests. Confirms that:
/// (a) V1-shaped PayoutExecutedEventSchema (no IdempotencyKey) reduces into
///     a PayoutReadModel with status "Executed" and an empty IdempotencyKey;
/// (b) V2-shaped (with key + timestamp) populates IdempotencyKey faithfully;
/// (c) the Requested → Executed sequence updates status as expected, so the
///     T3.5.4 JSONB index on `state->>'idempotencyKey'` populates correctly.
/// </summary>
public sealed class PayoutProjectionReducerTests
{
    private static readonly TestIdGenerator IdGen = new();

    [Fact]
    public void Apply_V1ExecutedSchema_SetsStatusExecutedAndLeavesIdempotencyKeyEmpty()
    {
        var payoutId = IdGen.Generate("Reducer:V1:payout");
        var distributionId = IdGen.Generate("Reducer:V1:distribution");
        var schema = new PayoutExecutedEventSchema(payoutId, distributionId);

        var state = PayoutProjectionReducer.Apply(new PayoutReadModel(), schema);

        Assert.Equal(payoutId, state.PayoutId);
        Assert.Equal(distributionId, state.DistributionId);
        Assert.Equal("Executed", state.Status);
        Assert.Equal(string.Empty, state.IdempotencyKey);
    }

    [Fact]
    public void Apply_V2ExecutedSchema_PopulatesIdempotencyKey()
    {
        var payoutId = IdGen.Generate("Reducer:V2:payout");
        var distributionId = IdGen.Generate("Reducer:V2:distribution");
        var key = $"payout|{distributionId:N}|spv-1";
        var schema = new PayoutExecutedEventSchema(payoutId, distributionId)
        {
            IdempotencyKey = key,
            ExecutedAt = new DateTimeOffset(2026, 4, 17, 11, 30, 0, TimeSpan.Zero)
        };

        var state = PayoutProjectionReducer.Apply(new PayoutReadModel(), schema);

        Assert.Equal("Executed", state.Status);
        Assert.Equal(key, state.IdempotencyKey);
    }

    [Fact]
    public void Apply_RequestedThenExecuted_AdvancesStatusAndPersistsKey()
    {
        var payoutId = IdGen.Generate("Reducer:Lifecycle:payout");
        var distributionId = IdGen.Generate("Reducer:Lifecycle:distribution");
        var key = $"payout|{distributionId:N}|spv-lifecycle";

        var requested = new PayoutRequestedEventSchema(payoutId, distributionId, key,
            new DateTimeOffset(2026, 4, 17, 11, 25, 0, TimeSpan.Zero));
        var afterRequest = PayoutProjectionReducer.Apply(new PayoutReadModel(), requested);
        Assert.Equal("Requested", afterRequest.Status);
        Assert.Equal(key, afterRequest.IdempotencyKey);

        var executed = new PayoutExecutedEventSchema(payoutId, distributionId)
        {
            IdempotencyKey = key,
            ExecutedAt = new DateTimeOffset(2026, 4, 17, 11, 30, 0, TimeSpan.Zero)
        };
        var afterExecute = PayoutProjectionReducer.Apply(afterRequest, executed);
        Assert.Equal("Executed", afterExecute.Status);
        Assert.Equal(key, afterExecute.IdempotencyKey);
    }

    [Fact]
    public void Apply_FailedSchema_SetsStatusFailed()
    {
        var payoutId = IdGen.Generate("Reducer:Failed:payout");
        var distributionId = IdGen.Generate("Reducer:Failed:distribution");
        var schema = new PayoutFailedEventSchema(payoutId, distributionId, "vault-debit-rejected",
            new DateTimeOffset(2026, 4, 17, 11, 35, 0, TimeSpan.Zero));

        var state = PayoutProjectionReducer.Apply(new PayoutReadModel(), schema);

        Assert.Equal("Failed", state.Status);
    }
}
