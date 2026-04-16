using Whycespace.Domain.EconomicSystem.Risk.Exposure;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.EconomicSystem.Risk.Exposure;

public sealed class ExposureAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp FrozenNow = new(new DateTimeOffset(2026, 4, 7, 12, 0, 0, TimeSpan.Zero));

    private static ExposureAggregate NewActive(decimal amount, string seed = "default")
    {
        var exposureId = new ExposureId(IdGen.Generate($"ExposureAggregateTests:{seed}:exposure"));
        var sourceId = new SourceId(IdGen.Generate($"ExposureAggregateTests:{seed}:source"));
        return ExposureAggregate.Create(
            exposureId,
            sourceId,
            ExposureType.Allocation,
            new Amount(amount),
            new Currency("USD"),
            FrozenNow);
    }

    [Fact]
    public void Create_WithValidAmount_RaisesCreatedEventAndSetsActive()
    {
        var aggregate = NewActive(100m, "Create_Valid");

        Assert.Equal(ExposureStatus.Active, aggregate.Status);
        Assert.Equal(100m, aggregate.TotalExposure.Value);

        var evt = Assert.IsType<ExposureCreatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(100m, evt.TotalExposure.Value);
        Assert.Equal("USD", evt.Currency.Code);
        Assert.Equal(ExposureType.Allocation, evt.ExposureType);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-50)]
    public void Create_WithZeroOrNegativeAmount_Throws(decimal amount)
    {
        var exposureId = new ExposureId(IdGen.Generate($"ExposureAggregateTests:Create_Invalid:{amount}"));
        var sourceId = new SourceId(IdGen.Generate("ExposureAggregateTests:Create_Invalid:source"));

        Assert.Throws<DomainException>(() =>
            ExposureAggregate.Create(
                exposureId,
                sourceId,
                ExposureType.Allocation,
                new Amount(amount),
                new Currency("USD"),
                FrozenNow));
    }

    [Fact]
    public void Increase_OnActive_AddsToTotalAndStaysActive()
    {
        var aggregate = NewActive(100m, "Increase");
        aggregate.ClearDomainEvents();

        aggregate.IncreaseExposure(new Amount(50m));

        Assert.Equal(150m, aggregate.TotalExposure.Value);
        Assert.Equal(ExposureStatus.Active, aggregate.Status);

        var evt = Assert.IsType<ExposureIncreasedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(50m, evt.IncreasedBy.Value);
        Assert.Equal(150m, evt.NewTotal.Value);
    }

    [Fact]
    public void Reduce_OnActive_TransitionsToReduced()
    {
        var aggregate = NewActive(100m, "Reduce");
        aggregate.ClearDomainEvents();

        aggregate.ReduceExposure(new Amount(40m));

        Assert.Equal(60m, aggregate.TotalExposure.Value);
        Assert.Equal(ExposureStatus.Reduced, aggregate.Status);

        var evt = Assert.IsType<ExposureReducedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(40m, evt.ReducedBy.Value);
        Assert.Equal(60m, evt.NewTotal.Value);
    }

    [Fact]
    public void Increase_AfterReduce_Reactivates()
    {
        var aggregate = NewActive(100m, "Reactivate");
        aggregate.ReduceExposure(new Amount(30m));
        Assert.Equal(ExposureStatus.Reduced, aggregate.Status);

        aggregate.IncreaseExposure(new Amount(20m));

        Assert.Equal(ExposureStatus.Active, aggregate.Status);
        Assert.Equal(90m, aggregate.TotalExposure.Value);
    }

    [Theory]
    [InlineData(nameof(ExposureStatus.Active))]
    [InlineData(nameof(ExposureStatus.Reduced))]
    public void Close_FromActiveOrReduced_TransitionsToClosedAndZeros(string fromStatus)
    {
        var aggregate = NewActive(100m, $"Close_{fromStatus}");
        if (fromStatus == nameof(ExposureStatus.Reduced))
            aggregate.ReduceExposure(new Amount(10m));

        aggregate.CloseExposure();

        Assert.Equal(ExposureStatus.Closed, aggregate.Status);
        Assert.Equal(0m, aggregate.TotalExposure.Value);
        Assert.Contains(aggregate.DomainEvents, e => e is ExposureClosedEvent);
    }

    [Fact]
    public void AllOperations_AfterClose_Throw()
    {
        var aggregate = NewActive(100m, "PostClose");
        aggregate.CloseExposure();

        Assert.Throws<DomainException>(() => aggregate.IncreaseExposure(new Amount(10m)));
        Assert.Throws<DomainException>(() => aggregate.ReduceExposure(new Amount(10m)));
        Assert.Throws<DomainException>(() => aggregate.CloseExposure());
    }

    [Fact]
    public void Reduce_ExceedingTotal_Throws()
    {
        var aggregate = NewActive(50m, "ReduceTooMuch");

        Assert.Throws<DomainException>(() =>
            aggregate.ReduceExposure(new Amount(60m)));
    }

    [Fact]
    public void LoadFromHistory_AfterFullLifecycle_ReconstitutesFinalState()
    {
        var exposureId = new ExposureId(IdGen.Generate("ExposureAggregateTests:History:exposure"));
        var sourceId = new SourceId(IdGen.Generate("ExposureAggregateTests:History:source"));

        var history = new object[]
        {
            new ExposureCreatedEvent(
                exposureId, sourceId, ExposureType.Obligation,
                new Amount(200m), new Currency("EUR"), FrozenNow),
            new ExposureIncreasedEvent(exposureId, new Amount(100m), new Amount(300m)),
            new ExposureReducedEvent(exposureId, new Amount(50m), new Amount(250m)),
        };

        var aggregate = (ExposureAggregate)Activator.CreateInstance(typeof(ExposureAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(exposureId, aggregate.ExposureId);
        Assert.Equal(ExposureType.Obligation, aggregate.ExposureType);
        Assert.Equal(250m, aggregate.TotalExposure.Value);
        Assert.Equal(ExposureStatus.Reduced, aggregate.Status);
        Assert.Equal("EUR", aggregate.Currency.Code);
        Assert.Equal(2, aggregate.Version); // 3 events → version 2 (0-indexed)
        Assert.Empty(aggregate.DomainEvents);
    }
}
