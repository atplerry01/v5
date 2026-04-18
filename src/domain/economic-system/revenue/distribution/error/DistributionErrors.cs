using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Revenue.Distribution;

public static class DistributionErrors
{
    public static DomainException InvalidTransition(DistributionStatus from, DistributionStatus to) =>
        new($"Distribution transition not allowed: {from} -> {to}.");

    public static DomainException AlreadyConfirmed() =>
        new("Distribution has already been confirmed.");

    public static DomainException AlreadyTerminal() =>
        new("Distribution is in a terminal state and cannot transition further.");

    public static DomainException PayoutIdRequired() =>
        new("PayoutId is required to mark a distribution as Paid.");

    public static DomainException CompensationNotAllowed(DistributionStatus status) =>
        new($"Compensation is only allowed from Paid or Failed state (was {status}).");

    public static DomainException AlreadyCompensated() =>
        new("Distribution has already been compensated.");

    public static DomainException CompensationCorrelationRequired() =>
        new("Compensation requires the originating PayoutId for correlation.");

    public static DomainException CompensatingJournalIdRequired() =>
        new("CompensatingJournalId is required to mark a distribution as Compensated.");

    public static DomainException CompensationNotRequested() =>
        new("Distribution cannot be marked Compensated without a prior CompensationRequested event.");
}
