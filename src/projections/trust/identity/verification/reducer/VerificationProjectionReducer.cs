using Whycespace.Shared.Contracts.Events.Trust.Identity.Verification;
using Whycespace.Shared.Contracts.Trust.Identity.Verification;

namespace Whycespace.Projections.Trust.Identity.Verification.Reducer;

public static class VerificationProjectionReducer
{
    public static VerificationReadModel Apply(VerificationReadModel state, VerificationInitiatedEventSchema e)
        => state with
        {
            VerificationId = e.AggregateId,
            IdentityReference = e.IdentityReference,
            ClaimType = e.ClaimType,
            Status = "Initiated",
            InitiatedAt = DateTimeOffset.UtcNow,
            LastUpdatedAt = DateTimeOffset.UtcNow
        };

    public static VerificationReadModel Apply(VerificationReadModel state, VerificationPassedEventSchema _)
        => state with { Status = "Passed", LastUpdatedAt = DateTimeOffset.UtcNow };

    public static VerificationReadModel Apply(VerificationReadModel state, VerificationFailedEventSchema _)
        => state with { Status = "Failed", LastUpdatedAt = DateTimeOffset.UtcNow };
}
