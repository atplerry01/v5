using System.Text.Json;
using Whycespace.Domain.EconomicSystem.Revenue.Payout;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Events.Economic.Revenue.Payout;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.EconomicSystem.Revenue.Payout;

/// <summary>
/// Phase 3.5 T3.5.1 / T3.5.5 — proves the additive-evolution path:
/// V1 wire shapes (`{ "PayoutId", "DistributionId" }` for the domain event;
/// `{ "AggregateId", "DistributionId" }` for the schema record) deserialize
/// cleanly into the new types and replay onto PayoutAggregate without
/// throwing or corrupting state. Catches any future change that turns
/// IdempotencyKey or ExecutedAt back into required positional ctor params.
/// </summary>
public sealed class PayoutEventReplayCompatibilityTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly JsonSerializerOptions WriteSideJson = new()
    {
        PropertyNamingPolicy = null, // event-store row uses PascalCase property names
        WriteIndented = false,
    };
    private static readonly JsonSerializerOptions ReadSideJson = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };

    [Fact]
    public void V1_DomainEventJson_DeserializesAndReplaysOntoPayoutAggregate()
    {
        var payoutId = new PayoutId(IdGen.Generate("Replay:V1:payout"));
        var distributionId = new DistributionId(IdGen.Generate("Replay:V1:distribution"));

        // V1 shape — only the two positional ctor fields. No IdempotencyKey
        // or ExecutedAt keys; legacy event-store rows look exactly like this.
        var v1Json = $"{{\"PayoutId\":\"{payoutId.Value}\",\"DistributionId\":\"{distributionId.Value}\"}}";

        var executed = JsonSerializer.Deserialize<PayoutExecutedEvent>(v1Json, WriteSideJson)!;

        Assert.Equal(payoutId.Value.ToString(), executed.PayoutId);
        Assert.Equal(distributionId.Value.ToString(), executed.DistributionId);
        Assert.Equal(string.Empty, executed.IdempotencyKey);
        Assert.Equal(default(Timestamp), executed.ExecutedAt);

        // Replay V1 stream: no PayoutRequestedEvent precursor — the Apply
        // switch must hydrate identifiers from the executed event itself
        // and stamp IdempotencyKey = "legacy".
        var replayed = (PayoutAggregate)Activator.CreateInstance(typeof(PayoutAggregate), nonPublic: true)!;
        replayed.LoadFromHistory(new object[] { executed });

        Assert.Equal(payoutId, replayed.PayoutId);
        Assert.Equal(distributionId, replayed.DistributionId);
        Assert.Equal(PayoutStatus.Executed, replayed.Status);
        Assert.Equal("legacy", replayed.IdempotencyKey.Value);
    }

    [Fact]
    public void V2_DomainEventJson_DeserializesWithAllFieldsAndReplaysCleanly()
    {
        var payoutId = new PayoutId(IdGen.Generate("Replay:V2:payout"));
        var distributionId = new DistributionId(IdGen.Generate("Replay:V2:distribution"));
        var idempotencyKey = $"payout|{distributionId.Value:N}|spv-v2";
        var executedAt = new DateTimeOffset(2026, 4, 17, 11, 30, 0, TimeSpan.Zero);

        var v2 = new PayoutExecutedEvent(payoutId.Value.ToString(), distributionId.Value.ToString())
        {
            IdempotencyKey = idempotencyKey,
            ExecutedAt = new Timestamp(executedAt)
        };
        var v2Json = JsonSerializer.Serialize(v2, WriteSideJson);
        var roundTripped = JsonSerializer.Deserialize<PayoutExecutedEvent>(v2Json, WriteSideJson)!;

        Assert.Equal(idempotencyKey, roundTripped.IdempotencyKey);
        Assert.Equal(new Timestamp(executedAt), roundTripped.ExecutedAt);
    }

    [Fact]
    public void V1_SchemaRecordJson_DeserializesWithoutThrowing()
    {
        // V1 schema wire shape — what an old Kafka payload looks like after
        // payload mapping. Reducer/projection consumers receive this.
        var payoutId = IdGen.Generate("Replay:V1Schema:payout");
        var distributionId = IdGen.Generate("Replay:V1Schema:distribution");
        var v1Json = $"{{\"aggregateId\":\"{payoutId}\",\"distributionId\":\"{distributionId}\"}}";

        var schema = JsonSerializer.Deserialize<PayoutExecutedEventSchema>(v1Json, ReadSideJson)!;

        Assert.Equal(payoutId, schema.AggregateId);
        Assert.Equal(distributionId, schema.DistributionId);
        Assert.Equal(string.Empty, schema.IdempotencyKey);
        Assert.Equal(default(DateTimeOffset), schema.ExecutedAt);
    }
}
