namespace Whycespace.Domain.ConstitutionalSystem.Policy.Violation;

using Whycespace.Domain.SharedKernel;

public sealed class PolicyViolationAggregate : AggregateRoot
{
    public Guid PolicyRuleId { get; private set; }
    public Guid ConstraintId { get; private set; }
    public ViolationSeverity Severity { get; private set; } = default!;
    public ViolationStatus Status { get; private set; } = default!;
    public string Description { get; private set; } = string.Empty;
    public string TargetEntity { get; private set; } = string.Empty;
    public DateTimeOffset DetectedAt { get; private set; }
    public DateTimeOffset? ResolvedAt { get; private set; }
    public string? ResolutionNote { get; private set; }

    private PolicyViolationAggregate() { }

    public static PolicyViolationAggregate Detect(
        Guid violationId,
        Guid policyRuleId,
        Guid constraintId,
        ViolationSeverity severity,
        string description,
        string targetEntity,
        DateTimeOffset timestamp)
    {
        ArgumentNullException.ThrowIfNull(severity);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetEntity);

        var violation = new PolicyViolationAggregate
        {
            Id = violationId,
            PolicyRuleId = policyRuleId,
            ConstraintId = constraintId,
            Severity = severity,
            Status = ViolationStatus.Detected,
            Description = description,
            TargetEntity = targetEntity,
            DetectedAt = timestamp
        };

        violation.RaiseDomainEvent(new ViolationDetectedEvent(
            violation.Id, policyRuleId, constraintId, severity.Value, targetEntity));

        return violation;
    }

    public void Acknowledge()
    {
        if (Status != ViolationStatus.Detected)
            throw new InvalidOperationException("Only detected violations can be acknowledged.");

        Status = ViolationStatus.Acknowledged;
        RaiseDomainEvent(new ViolationAcknowledgedEvent(Id));
    }

    public void Resolve(string resolutionNote, DateTimeOffset timestamp)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resolutionNote);

        if (Status == ViolationStatus.Resolved)
            throw new InvalidOperationException("Violation is already resolved.");

        Status = ViolationStatus.Resolved;
        ResolvedAt = timestamp;
        ResolutionNote = resolutionNote;
        RaiseDomainEvent(new ViolationResolvedEvent(Id, resolutionNote));
    }

    public void Escalate()
    {
        if (Status == ViolationStatus.Resolved)
            throw new InvalidOperationException("Resolved violations cannot be escalated.");

        Status = ViolationStatus.Escalated;
        RaiseDomainEvent(new ViolationEscalatedEvent(Id, Severity.Value));
    }

    public bool IsOpen => Status != ViolationStatus.Resolved;
}
