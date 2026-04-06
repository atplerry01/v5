namespace Whycespace.Domain.IntelligenceSystem.Observability.Metric;

/// <summary>
/// Policy evaluation and enforcement observability metrics.
/// No business logic — observability only.
/// </summary>
public sealed class PolicyMetrics
{
    private long _evaluationCount;
    private long _denyCount;
    private long _allowCount;
    private long _conditionalCount;
    private long _violationCount;
    private long _replayMismatchCount;
    private long _totalLatencyMs;

    // Enforcement metrics
    private long _enforcementTriggeredCount;
    private long _enforcementWarningCount;
    private long _enforcementRestrictionCount;
    private long _enforcementFreezeCount;
    private long _enforcementBlockCount;
    private long _enforcementHaltCount;
    private long _enforcementAuditTriggerCount;
    private long _enforcementFailureCount;

    public void RecordEvaluation(string decisionType, long latencyMs)
    {
        Interlocked.Increment(ref _evaluationCount);
        Interlocked.Add(ref _totalLatencyMs, latencyMs);

        switch (decisionType)
        {
            case "ALLOW": Interlocked.Increment(ref _allowCount); break;
            case "DENY": Interlocked.Increment(ref _denyCount); break;
            case "CONDITIONAL": Interlocked.Increment(ref _conditionalCount); break;
        }
    }

    public void RecordViolation(int count = 1) => Interlocked.Add(ref _violationCount, count);
    public void RecordReplayMismatch() => Interlocked.Increment(ref _replayMismatchCount);

    public void RecordEnforcement(string enforcementType)
    {
        Interlocked.Increment(ref _enforcementTriggeredCount);

        switch (enforcementType)
        {
            case "warning": Interlocked.Increment(ref _enforcementWarningCount); break;
            case "restriction": Interlocked.Increment(ref _enforcementRestrictionCount); break;
            case "freeze": Interlocked.Increment(ref _enforcementFreezeCount); break;
            case "block": Interlocked.Increment(ref _enforcementBlockCount); break;
            case "halt": Interlocked.Increment(ref _enforcementHaltCount); break;
            case "audit_trigger": Interlocked.Increment(ref _enforcementAuditTriggerCount); break;
        }
    }

    public void RecordEnforcementFailure() => Interlocked.Increment(ref _enforcementFailureCount);

    // Activation metrics
    private long _activationSuccessCount;
    private long _activationFailureCount;

    // Registry cache metrics
    private long _registryCacheHitCount;
    private long _registryCacheMissCount;

    // Version resolution metrics
    private long _versionResolutionCount;
    private long _totalVersionResolutionLatencyMs;

    // Governance metrics
    private long _proposalCount;
    private long _approvalCount;
    private long _rejectionCount;
    private long _quorumFailureCount;
    private long _unauthorizedAttemptCount;
    private long _emergencyOverrideCount;

    public void RecordActivationSuccess() => Interlocked.Increment(ref _activationSuccessCount);
    public void RecordActivationFailure() => Interlocked.Increment(ref _activationFailureCount);
    public void RecordRegistryCacheHit() => Interlocked.Increment(ref _registryCacheHitCount);
    public void RecordRegistryCacheMiss() => Interlocked.Increment(ref _registryCacheMissCount);

    public void RecordVersionResolution(long latencyMs)
    {
        Interlocked.Increment(ref _versionResolutionCount);
        Interlocked.Add(ref _totalVersionResolutionLatencyMs, latencyMs);
    }

    public void RecordProposal() => Interlocked.Increment(ref _proposalCount);
    public void RecordApproval() => Interlocked.Increment(ref _approvalCount);
    public void RecordRejection() => Interlocked.Increment(ref _rejectionCount);
    public void RecordQuorumFailure() => Interlocked.Increment(ref _quorumFailureCount);
    public void RecordUnauthorizedAttempt() => Interlocked.Increment(ref _unauthorizedAttemptCount);
    public void RecordEmergencyOverride() => Interlocked.Increment(ref _emergencyOverrideCount);

    public PolicyMetricsSnapshot GetSnapshot() => new()
    {
        EvaluationCount = Interlocked.Read(ref _evaluationCount),
        AllowCount = Interlocked.Read(ref _allowCount),
        DenyCount = Interlocked.Read(ref _denyCount),
        ConditionalCount = Interlocked.Read(ref _conditionalCount),
        ViolationCount = Interlocked.Read(ref _violationCount),
        ReplayMismatchCount = Interlocked.Read(ref _replayMismatchCount),
        AverageLatencyMs = _evaluationCount > 0
            ? (double)Interlocked.Read(ref _totalLatencyMs) / Interlocked.Read(ref _evaluationCount) : 0,
        EnforcementTriggeredCount = Interlocked.Read(ref _enforcementTriggeredCount),
        EnforcementByType = new Dictionary<string, long>
        {
            ["warning"] = Interlocked.Read(ref _enforcementWarningCount),
            ["restriction"] = Interlocked.Read(ref _enforcementRestrictionCount),
            ["freeze"] = Interlocked.Read(ref _enforcementFreezeCount),
            ["block"] = Interlocked.Read(ref _enforcementBlockCount),
            ["halt"] = Interlocked.Read(ref _enforcementHaltCount),
            ["audit_trigger"] = Interlocked.Read(ref _enforcementAuditTriggerCount)
        },
        EnforcementFailureCount = Interlocked.Read(ref _enforcementFailureCount),
        AutoFreezeCount = Interlocked.Read(ref _enforcementFreezeCount),
        AuditTriggerCount = Interlocked.Read(ref _enforcementAuditTriggerCount),
        ActivationSuccessCount = Interlocked.Read(ref _activationSuccessCount),
        ActivationFailureCount = Interlocked.Read(ref _activationFailureCount),
        RegistryCacheHitCount = Interlocked.Read(ref _registryCacheHitCount),
        RegistryCacheMissCount = Interlocked.Read(ref _registryCacheMissCount),
        VersionResolutionCount = Interlocked.Read(ref _versionResolutionCount),
        AverageVersionResolutionLatencyMs = _versionResolutionCount > 0
            ? (double)Interlocked.Read(ref _totalVersionResolutionLatencyMs) / Interlocked.Read(ref _versionResolutionCount) : 0,
        ProposalCount = Interlocked.Read(ref _proposalCount),
        ApprovalCount = Interlocked.Read(ref _approvalCount),
        RejectionCount = Interlocked.Read(ref _rejectionCount),
        QuorumFailureCount = Interlocked.Read(ref _quorumFailureCount),
        UnauthorizedAttemptCount = Interlocked.Read(ref _unauthorizedAttemptCount),
        EmergencyOverrideCount = Interlocked.Read(ref _emergencyOverrideCount)
    };
}

public sealed record PolicyMetricsSnapshot
{
    public long EvaluationCount { get; init; }
    public long AllowCount { get; init; }
    public long DenyCount { get; init; }
    public long ConditionalCount { get; init; }
    public long ViolationCount { get; init; }
    public long ReplayMismatchCount { get; init; }
    public double AverageLatencyMs { get; init; }
    public long EnforcementTriggeredCount { get; init; }
    public IReadOnlyDictionary<string, long> EnforcementByType { get; init; } = new Dictionary<string, long>();
    public long EnforcementFailureCount { get; init; }
    public long AutoFreezeCount { get; init; }
    public long AuditTriggerCount { get; init; }

    // Activation
    public long ActivationSuccessCount { get; init; }
    public long ActivationFailureCount { get; init; }

    // Registry cache
    public long RegistryCacheHitCount { get; init; }
    public long RegistryCacheMissCount { get; init; }

    // Version resolution
    public long VersionResolutionCount { get; init; }
    public double AverageVersionResolutionLatencyMs { get; init; }

    // Governance
    public long ProposalCount { get; init; }
    public long ApprovalCount { get; init; }
    public long RejectionCount { get; init; }
    public long QuorumFailureCount { get; init; }
    public long UnauthorizedAttemptCount { get; init; }
    public long EmergencyOverrideCount { get; init; }

    public double DenyRate => EvaluationCount > 0
        ? (double)DenyCount / EvaluationCount * 100 : 0;
}
