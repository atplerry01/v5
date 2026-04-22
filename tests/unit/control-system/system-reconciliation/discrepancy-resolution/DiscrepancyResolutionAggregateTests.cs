using Whycespace.Domain.ControlSystem.SystemReconciliation.DiscrepancyDetection;
using Whycespace.Domain.ControlSystem.SystemReconciliation.DiscrepancyResolution;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.ControlSystem.SystemReconciliation.DiscrepancyResolution;

public sealed class DiscrepancyResolutionAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly DateTimeOffset InitiatedAt = new(2026, 4, 22, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset CompletedAt = new(2026, 4, 22, 11, 0, 0, TimeSpan.Zero);

    private static string Hex64(string seed)
    {
        var g = IdGen.Generate(seed);
        return g.ToString("N") + g.ToString("N");
    }

    private static DiscrepancyResolutionId NewId(string seed) =>
        new(Hex64($"DiscrepancyResolutionTests:{seed}:resol"));

    private static DiscrepancyDetectionId NewDetectionId(string seed) =>
        new(Hex64($"DiscrepancyResolutionTests:{seed}:detect"));

    [Fact]
    public void Initiate_RaisesDiscrepancyResolutionInitiatedEvent()
    {
        var id = NewId("Initiate");
        var detectionId = NewDetectionId("Initiate");

        var aggregate = DiscrepancyResolutionAggregate.Initiate(id, detectionId, InitiatedAt);

        var evt = Assert.IsType<DiscrepancyResolutionInitiatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.Id);
        Assert.Equal(detectionId, evt.DetectionId);
    }

    [Fact]
    public void Initiate_SetsStatusToInitiated()
    {
        var aggregate = DiscrepancyResolutionAggregate.Initiate(NewId("State"), NewDetectionId("State"), InitiatedAt);

        Assert.Equal(ResolutionStatus.Initiated, aggregate.Status);
        Assert.Null(aggregate.Outcome);
    }

    [Fact]
    public void Complete_RaisesDiscrepancyResolutionCompletedEvent()
    {
        var aggregate = DiscrepancyResolutionAggregate.Initiate(NewId("Complete"), NewDetectionId("Complete"), InitiatedAt);
        aggregate.ClearDomainEvents();

        aggregate.Complete(ResolutionOutcome.Corrected, "Applied patch to fix mismatch", CompletedAt);

        var evt = Assert.IsType<DiscrepancyResolutionCompletedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(ResolutionOutcome.Corrected, evt.Outcome);
        Assert.Equal(ResolutionStatus.Completed, aggregate.Status);
        Assert.Equal(ResolutionOutcome.Corrected, aggregate.Outcome);
    }

    [Fact]
    public void Complete_WithEmptyNotes_Throws()
    {
        var aggregate = DiscrepancyResolutionAggregate.Initiate(NewId("EmptyNotes"), NewDetectionId("EmptyNotes"), InitiatedAt);

        Assert.ThrowsAny<Exception>(() =>
            aggregate.Complete(ResolutionOutcome.Accepted, "", CompletedAt));
    }

    [Fact]
    public void Complete_AlreadyCompleted_Throws()
    {
        var aggregate = DiscrepancyResolutionAggregate.Initiate(NewId("DoubleComplete"), NewDetectionId("DoubleComplete"), InitiatedAt);
        aggregate.Complete(ResolutionOutcome.Rejected, "Rejected by policy", CompletedAt);

        Assert.ThrowsAny<Exception>(() =>
            aggregate.Complete(ResolutionOutcome.Corrected, "notes", CompletedAt.AddMinutes(5)));
    }

    [Fact]
    public void LoadFromHistory_RehydratesState()
    {
        var id = NewId("History");
        var detectionId = NewDetectionId("History");

        var history = new object[]
        {
            new DiscrepancyResolutionInitiatedEvent(id, detectionId, InitiatedAt)
        };
        var aggregate = (DiscrepancyResolutionAggregate)Activator.CreateInstance(typeof(DiscrepancyResolutionAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal(detectionId, aggregate.DetectionId);
        Assert.Equal(ResolutionStatus.Initiated, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }
}
