using Whycespace.Domain.EconomicSystem.Revenue.Revenue;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.EconomicSystem.Revenue.Revenue;

public sealed class RevenueTests
{
    private static readonly TestIdGenerator IdGen = new();

    [Fact]
    public void RecordRevenue_WithValidInputs_EmitsEventAndSetsRecordedStatus()
    {
        var revenueId = RevenueId.From(IdGen.Generate("RevenueTests:Record:revenue"));
        const string spvId = "spv-001";
        const decimal amount = 5_000m;
        const string currency = "USD";
        const string sourceRef = "invoice-2026-042";

        var aggregate = RevenueAggregate.RecordRevenue(
            revenueId, spvId, amount, currency, sourceRef);

        Assert.Equal(RevenueStatus.Recorded, aggregate.Status);
        Assert.Equal(spvId, aggregate.SpvId);
        Assert.Equal(amount, aggregate.Amount.Value);
        Assert.Equal(currency, aggregate.Currency.Code);
        Assert.Equal(sourceRef, aggregate.SourceRef);

        var evt = Assert.IsType<RevenueRecordedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(revenueId.Value.ToString(), evt.RevenueId);
        Assert.Equal(spvId, evt.SpvId);
        Assert.Equal(amount, evt.Amount);
        Assert.Equal(currency, evt.Currency);
        Assert.Equal(sourceRef, evt.SourceRef);
    }

    [Fact]
    public void RecordRevenue_WithNonPositiveAmount_Throws()
    {
        var revenueId = RevenueId.From(IdGen.Generate("RevenueTests:Invalid:revenue"));

        Assert.Throws<ArgumentException>(() =>
            RevenueAggregate.RecordRevenue(revenueId, "spv-001", 0m, "USD", "x"));
    }
}
