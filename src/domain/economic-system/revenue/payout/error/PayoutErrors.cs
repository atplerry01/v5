using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Revenue.Payout;

public static class PayoutErrors
{
    public static DomainException MissingDistributionReference() =>
        new("Payout must reference a distribution.");

    public static DomainException PayoutNotPending() =>
        new("Payout must be in Pending status.");

    public static DomainException PayoutAlreadyCompleted() =>
        new("Payout has already been completed.");

    public static DomainException PayoutAlreadyFailed() =>
        new("Payout has already failed.");

    public static DomainException CannotCompleteFailedPayout() =>
        new("Cannot complete a failed payout.");

    public static DomainException CannotFailCompletedPayout() =>
        new("Cannot fail a completed payout.");
}
