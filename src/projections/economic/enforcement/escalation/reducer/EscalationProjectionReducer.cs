using Whycespace.Shared.Contracts.Economic.Enforcement.Escalation;
using Whycespace.Shared.Contracts.Events.Economic.Enforcement.Escalation;

namespace Whycespace.Projections.Economic.Enforcement.Escalation.Reducer;

public static class EscalationProjectionReducer
{
    public static EscalationReadModel Apply(EscalationReadModel state, EscalationInitializedEventSchema e) =>
        state with
        {
            SubjectId = e.AggregateId,
            ViolationCount = 0,
            SeverityScore = 0,
            EscalationLevel = "None",
            WindowStart = e.WindowStart,
            LastViolationAt = e.InitializedAt,
            LastUpdatedAt = e.InitializedAt,
            Version = state.Version + 1
        };

    public static EscalationReadModel Apply(EscalationReadModel state, EscalationResetEventSchema e) =>
        state with
        {
            ViolationCount = 0,
            SeverityScore = 0,
            EscalationLevel = "None",
            WindowStart = e.NewWindowStart,
            LastViolationAt = e.ResetAt,
            LastUpdatedAt = e.ResetAt,
            Version = state.Version + 1
        };

    public static EscalationReadModel Apply(EscalationReadModel state, ViolationAccumulatedEventSchema e) =>
        state with
        {
            ViolationCount = e.NewCount,
            SeverityScore = e.NewSeverityScore,
            LastViolationId = e.ViolationId,
            LastViolationAt = e.AccumulatedAt,
            LastUpdatedAt = e.AccumulatedAt,
            Version = state.Version + 1
        };

    public static EscalationReadModel Apply(EscalationReadModel state, EscalationLevelIncreasedEventSchema e) =>
        state with
        {
            EscalationLevel = e.NewLevel,
            ViolationCount = e.Count,
            SeverityScore = e.SeverityScore,
            LastUpdatedAt = e.EscalatedAt,
            Version = state.Version + 1
        };
}
