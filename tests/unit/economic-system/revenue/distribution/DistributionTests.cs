using Whycespace.Domain.EconomicSystem.Revenue.Distribution;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.EconomicSystem.Revenue.Distribution;

public sealed class DistributionTests
{
    private static readonly TestIdGenerator IdGen = new();

    [Fact]
    public void CreateDistribution_ComputesShareAmountsByOwnershipPercentage()
    {
        var distributionId = DistributionId.From(IdGen.Generate("DistributionTests:Compute:distribution"));
        const string spvId = "spv-001";
        const decimal totalAmount = 1_000m;

        var allocations = new[]
        {
            ("participant-a", 60m),
            ("participant-b", 40m),
        };

        var aggregate = DistributionAggregate.CreateDistribution(
            distributionId, spvId, totalAmount, allocations);

        Assert.Equal(DistributionStatus.Created, aggregate.Status);
        Assert.Equal(totalAmount, aggregate.TotalAmount.Value);
        Assert.Equal(2, aggregate.Shares.Count);

        var a = aggregate.Shares.Single(s => s.ParticipantId == "participant-a");
        var b = aggregate.Shares.Single(s => s.ParticipantId == "participant-b");
        Assert.Equal(600m, a.Amount);
        Assert.Equal(400m, b.Amount);
        Assert.Equal(totalAmount, a.Amount + b.Amount);

        Assert.IsType<DistributionCreatedEvent>(Assert.Single(aggregate.DomainEvents));
    }

    [Fact]
    public void CreateDistribution_WhenPercentagesDoNotSum100_Throws()
    {
        var distributionId = DistributionId.From(IdGen.Generate("DistributionTests:BadSum:distribution"));
        var allocations = new[]
        {
            ("participant-a", 50m),
            ("participant-b", 30m),
        };

        Assert.Throws<ArgumentException>(() =>
            DistributionAggregate.CreateDistribution(
                distributionId, "spv-001", 1_000m, allocations));
    }
}
