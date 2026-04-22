using Whycespace.Shared.Contracts.Control.SystemReconciliation.SystemVerification;
using Whycespace.Shared.Contracts.Events.Control.SystemReconciliation.SystemVerification;

namespace Whycespace.Projections.Control.SystemReconciliation.SystemVerification.Reducer;

public static class SystemVerificationProjectionReducer
{
    public static SystemVerificationReadModel Apply(SystemVerificationReadModel state, SystemVerificationInitiatedEventSchema e) =>
        state with
        {
            VerificationId = e.AggregateId,
            TargetSystem   = e.TargetSystem,
            Status         = "Initiated",
            InitiatedAt    = e.InitiatedAt
        };

    public static SystemVerificationReadModel Apply(SystemVerificationReadModel state, SystemVerificationPassedEventSchema e) =>
        state with
        {
            Status   = "Passed",
            PassedAt = e.PassedAt
        };

    public static SystemVerificationReadModel Apply(SystemVerificationReadModel state, SystemVerificationFailedEventSchema e) =>
        state with
        {
            Status        = "Failed",
            FailureReason = e.FailureReason,
            FailedAt      = e.FailedAt
        };
}
