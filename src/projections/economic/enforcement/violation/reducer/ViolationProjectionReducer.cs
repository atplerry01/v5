using Whycespace.Shared.Contracts.Economic.Enforcement.Violation;
using Whycespace.Shared.Contracts.Events.Economic.Enforcement.Violation;

namespace Whycespace.Projections.Economic.Enforcement.Violation.Reducer;

public static class ViolationProjectionReducer
{
    public static ViolationReadModel Apply(ViolationReadModel state, ViolationDetectedEventSchema e) =>
        state with
        {
            ViolationId = e.AggregateId,
            RuleId = e.RuleId,
            SourceReference = e.SourceReference,
            Reason = e.Reason,
            Status = "Open",
            Severity = e.Severity,
            Action = e.RecommendedAction,
            IsActive = true,
            EnforcementStatus = DeriveEnforcementStatus("Open", e.RecommendedAction),
            DetectedAt = e.DetectedAt,
            AcknowledgedAt = null,
            ResolvedAt = null,
            Resolution = string.Empty,
            LastUpdatedAt = e.DetectedAt
        };

    public static ViolationReadModel Apply(ViolationReadModel state, ViolationAcknowledgedEventSchema e) =>
        state with
        {
            ViolationId = e.AggregateId,
            Status = "Acknowledged",
            IsActive = true,
            EnforcementStatus = DeriveEnforcementStatus("Acknowledged", state.Action),
            AcknowledgedAt = e.AcknowledgedAt,
            LastUpdatedAt = e.AcknowledgedAt
        };

    public static ViolationReadModel Apply(ViolationReadModel state, ViolationResolvedEventSchema e) =>
        state with
        {
            ViolationId = e.AggregateId,
            Status = "Resolved",
            IsActive = false,
            EnforcementStatus = "Cleared",
            Resolution = e.Resolution,
            ResolvedAt = e.ResolvedAt,
            LastUpdatedAt = e.ResolvedAt
        };

    private static string DeriveEnforcementStatus(string status, string action) =>
        status switch
        {
            "Resolved" => "Cleared",
            _ => action switch
            {
                "Block"    => "Blocking",
                "Restrict" => "Restricting",
                "Escalate" => "Escalated",
                "Warn"     => "Warning",
                _          => "Pending"
            }
        };
}
