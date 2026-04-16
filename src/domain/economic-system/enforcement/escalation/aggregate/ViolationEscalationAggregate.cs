using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Enforcement.Escalation;

/// <summary>
/// Aggregates violations per subject into a replay-deterministic escalation
/// level. Identity is the subject itself (SubjectId == aggregate id) so a
/// given subject has exactly one escalation stream.
/// </summary>
public sealed class ViolationEscalationAggregate : AggregateRoot
{
    private static readonly EscalationThresholdSpecification Spec = new();

    public SubjectId SubjectId { get; private set; }
    public EscalationLevel Level { get; private set; }
    public ViolationCounter Counter { get; private set; }
    public EscalationWindow Window { get; private set; }
    public Timestamp LastViolationAt { get; private set; }

    private ViolationEscalationAggregate() { }

    public static ViolationEscalationAggregate Accumulate(
        ViolationEscalationAggregate? existing,
        SubjectId subjectId,
        Guid violationId,
        int severityWeight,
        Timestamp at)
    {
        if (subjectId.Value == Guid.Empty) throw EscalationErrors.MissingSubject();
        if (severityWeight <= 0) severityWeight = 1;

        var aggregate = existing ?? new ViolationEscalationAggregate();

        if (aggregate.Version < 0)
        {
            aggregate.RaiseDomainEvent(new EscalationInitializedEvent(
                subjectId, EscalationWindow.Open(at), at));
        }
        else if (aggregate.Window.IsExpired(at))
        {
            aggregate.RaiseDomainEvent(new EscalationResetEvent(
                subjectId, EscalationWindow.Open(at), at));
        }

        var newCounter = aggregate.Counter.Add(severityWeight);
        aggregate.RaiseDomainEvent(new ViolationAccumulatedEvent(
            subjectId, violationId, severityWeight, newCounter, at));

        var newLevel = Spec.Classify(newCounter);
        if ((int)newLevel > (int)aggregate.Level)
        {
            aggregate.RaiseDomainEvent(new EscalationLevelIncreasedEvent(
                subjectId, aggregate.Level, newLevel, newCounter, at));
        }

        return aggregate;
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case EscalationInitializedEvent e:
                SubjectId = e.SubjectId;
                Window = e.Window;
                Counter = ViolationCounter.Zero;
                Level = EscalationLevel.None;
                LastViolationAt = e.InitializedAt;
                break;

            case EscalationResetEvent e:
                Window = e.NewWindow;
                Counter = ViolationCounter.Zero;
                Level = EscalationLevel.None;
                LastViolationAt = e.ResetAt;
                break;

            case ViolationAccumulatedEvent e:
                Counter = e.NewCounter;
                LastViolationAt = e.AccumulatedAt;
                break;

            case EscalationLevelIncreasedEvent e:
                Level = e.NewLevel;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (SubjectId.Value == Guid.Empty) throw EscalationErrors.EmptySubject();
        if (Counter.Count < 0 || Counter.SeverityScore < 0) throw EscalationErrors.NegativeCounter();
    }
}
