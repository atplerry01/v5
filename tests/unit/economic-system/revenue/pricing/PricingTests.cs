using Whycespace.Domain.EconomicSystem.Revenue.Pricing;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.EconomicSystem.Revenue.Pricing;

public sealed class PricingTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp Now = new(new DateTimeOffset(2026, 4, 7, 12, 0, 0, TimeSpan.Zero));

    [Fact]
    public void DefinePrice_ThenAdjust_LatestPriceReflectsAdjustment()
    {
        var pricingId = PricingId.From(IdGen.Generate("PricingTests:Adjust:pricing"));
        var contractId = IdGen.Generate("PricingTests:Adjust:contract");
        var currency = new Currency("USD");

        var aggregate = PricingAggregate.DefinePrice(
            pricingId, contractId, PricingModel.Fixed, new Amount(100m), currency, Now);

        Assert.Equal(100m, aggregate.Price.Value);

        aggregate.AdjustPrice(new Amount(125m), "market correction", new Timestamp(Now.Value.AddDays(7)));

        Assert.Equal(125m, aggregate.Price.Value);
        Assert.Equal(2, aggregate.DomainEvents.Count);

        var adjusted = Assert.IsType<PriceAdjustedEvent>(aggregate.DomainEvents[1]);
        Assert.Equal(100m, adjusted.PreviousPrice.Value);
        Assert.Equal(125m, adjusted.NewPrice.Value);
        Assert.Equal("market correction", adjusted.Reason);
    }
}
