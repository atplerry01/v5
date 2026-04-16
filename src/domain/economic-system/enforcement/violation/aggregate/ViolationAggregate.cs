using Whycespace.Domain.EconomicSystem.Enforcement.Rule;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Enforcement.Violation;

public sealed class ViolationAggregate : AggregateRoot
{
    public ViolationId ViolationId { get; private set; }
    public RuleId RuleId { get; private set; }
    public SourceReference Source { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public ViolationSeverity Severity { get; private set; }
    public EnforcementAction RecommendedAction { get; private set; }
    public ViolationStatus Status { get; private set; }
    public Timestamp DetectedAt { get; private set; }

    private ViolationAggregate() { }

    // ── Factory ──────────────────────────────────────────────────

    public static ViolationAggregate Detect(
        ViolationId violationId,
        RuleId ruleId,
        SourceReference source,
        string reason,
        ViolationSeverity severity,
        EnforcementAction recommendedAction,
        Timestamp detectedAt)
    {
        if (ruleId.Value == Guid.Empty) throw ViolationErrors.MissingRuleReference();
        if (source.Value == Guid.Empty) throw ViolationErrors.MissingSourceReference();
        if (string.IsNullOrWhiteSpace(reason)) throw ViolationErrors.MissingReason();
        if (!IsValidCombination(severity, recommendedAction))
            throw ViolationErrors.InvalidSeverityActionCombination(severity, recommendedAction);

        var aggregate = new ViolationAggregate();
        aggregate.RaiseDomainEvent(new ViolationDetectedEvent(
            violationId, ruleId, source, reason, severity, recommendedAction, detectedAt));
        return aggregate;
    }

    // Severity/action pairing invariants: Critical cannot be merely Warn;
    // Block/Escalate must correspond to High or Critical severity.
    private static bool IsValidCombination(ViolationSeverity severity, EnforcementAction action) =>
        (severity, action) switch
        {
            (ViolationSeverity.Critical, EnforcementAction.Warn)                          => false,
            (ViolationSeverity.Low, EnforcementAction.Block)                              => false,
            (ViolationSeverity.Low, EnforcementAction.Escalate)                           => false,
            (ViolationSeverity.Medium, EnforcementAction.Block)                           => false,
            _                                                                             => true
        };

    // ── Behavior ─────────────────────────────────────────────────

    public void Acknowledge(Timestamp acknowledgedAt)
    {
        if (Status != ViolationStatus.Open) throw ViolationErrors.ViolationNotOpen();

        RaiseDomainEvent(new ViolationAcknowledgedEvent(ViolationId, acknowledgedAt));
    }

    public void Resolve(string resolution, Timestamp resolvedAt)
    {
        if (Status == ViolationStatus.Resolved) throw ViolationErrors.ViolationAlreadyResolved();
        if (Status != ViolationStatus.Acknowledged) throw ViolationErrors.ViolationNotAcknowledged();
        if (string.IsNullOrWhiteSpace(resolution)) throw ViolationErrors.MissingResolution();

        RaiseDomainEvent(new ViolationResolvedEvent(ViolationId, resolution, resolvedAt));
    }

    // ── Apply ────────────────────────────────────────────────────

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case ViolationDetectedEvent e:
                ViolationId = e.ViolationId;
                RuleId = e.RuleId;
                Source = e.Source;
                Reason = e.Reason;
                Severity = e.Severity;
                RecommendedAction = e.RecommendedAction;
                Status = ViolationStatus.Open;
                DetectedAt = e.DetectedAt;
                break;

            case ViolationAcknowledgedEvent:
                Status = ViolationStatus.Acknowledged;
                break;

            case ViolationResolvedEvent:
                Status = ViolationStatus.Resolved;
                break;
        }
    }

    // ── Invariants ───────────────────────────────────────────────

    protected override void EnsureInvariants()
    {
        if (ViolationId.Value == Guid.Empty) throw ViolationErrors.EmptyViolationId();
        if (RuleId.Value == Guid.Empty || Source.Value == Guid.Empty) throw ViolationErrors.OrphanViolation();
    }
}
