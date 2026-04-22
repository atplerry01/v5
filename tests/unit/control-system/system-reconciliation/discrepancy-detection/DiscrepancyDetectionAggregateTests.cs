using Whycespace.Domain.ControlSystem.SystemReconciliation.DiscrepancyDetection;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.ControlSystem.SystemReconciliation.DiscrepancyDetection;

public sealed class DiscrepancyDetectionAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly DateTimeOffset DetectedAt = new(2026, 4, 22, 10, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset DismissedAt = new(2026, 4, 22, 11, 0, 0, TimeSpan.Zero);

    private static string Hex64(string seed)
    {
        var g = IdGen.Generate(seed);
        return g.ToString("N") + g.ToString("N");
    }

    private static DiscrepancyDetectionId NewId(string seed) =>
        new(Hex64($"DiscrepancyDetectionTests:{seed}:detect"));

    [Fact]
    public void Detect_RaisesDiscrepancyDetectedEvent()
    {
        var id = NewId("Detect");

        var aggregate = DiscrepancyDetectionAggregate.Detect(id, DiscrepancyKind.ValueMismatch, "audit-log/entry-42", DetectedAt);

        var evt = Assert.IsType<DiscrepancyDetectedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(id, evt.Id);
        Assert.Equal(DiscrepancyKind.ValueMismatch, evt.Kind);
        Assert.Equal("audit-log/entry-42", evt.SourceReference);
    }

    [Fact]
    public void Detect_SetsStatusToDetected()
    {
        var aggregate = DiscrepancyDetectionAggregate.Detect(NewId("State"), DiscrepancyKind.MissingRecord, "ref-1", DetectedAt);

        Assert.Equal(DetectionStatus.Detected, aggregate.Status);
    }

    [Fact]
    public void Detect_WithEmptySourceReference_Throws()
    {
        Assert.ThrowsAny<Exception>(() =>
            DiscrepancyDetectionAggregate.Detect(NewId("EmptyRef"), DiscrepancyKind.ExtraRecord, "", DetectedAt));
    }

    [Fact]
    public void Dismiss_RaisesDiscrepancyDetectionDismissedEvent()
    {
        var aggregate = DiscrepancyDetectionAggregate.Detect(NewId("Dismiss"), DiscrepancyKind.SequenceGap, "ref-1", DetectedAt);
        aggregate.ClearDomainEvents();

        aggregate.Dismiss("known gap from migration", DismissedAt);

        Assert.IsType<DiscrepancyDetectionDismissedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal(DetectionStatus.Dismissed, aggregate.Status);
    }

    [Fact]
    public void Dismiss_WithEmptyReason_Throws()
    {
        var aggregate = DiscrepancyDetectionAggregate.Detect(NewId("EmptyReason"), DiscrepancyKind.IntegrityViolation, "ref-1", DetectedAt);

        Assert.ThrowsAny<Exception>(() => aggregate.Dismiss("", DismissedAt));
    }

    [Fact]
    public void Dismiss_AlreadyDismissed_Throws()
    {
        var aggregate = DiscrepancyDetectionAggregate.Detect(NewId("DoubleDismiss"), DiscrepancyKind.ValueMismatch, "ref-1", DetectedAt);
        aggregate.Dismiss("reason", DismissedAt);

        Assert.ThrowsAny<Exception>(() => aggregate.Dismiss("reason-2", DismissedAt.AddMinutes(5)));
    }

    [Fact]
    public void LoadFromHistory_RehydratesState()
    {
        var id = NewId("History");

        var history = new object[]
        {
            new DiscrepancyDetectedEvent(id, DiscrepancyKind.MissingRecord, "ref-1", DetectedAt)
        };
        var aggregate = (DiscrepancyDetectionAggregate)Activator.CreateInstance(typeof(DiscrepancyDetectionAggregate), nonPublic: true)!;
        aggregate.LoadFromHistory(history);

        Assert.Equal(id, aggregate.Id);
        Assert.Equal(DetectionStatus.Detected, aggregate.Status);
        Assert.Empty(aggregate.DomainEvents);
    }
}
