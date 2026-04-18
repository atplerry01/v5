using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Revenue.Payout;

public static class PayoutErrors
{
    public static DomainException InvalidTransition(PayoutStatus from, PayoutStatus to) =>
        new($"Payout transition not allowed: {from} -> {to}.");

    public static DomainException AlreadyTerminal() =>
        new("Payout is in a terminal state and cannot transition further.");

    public static DomainException IdempotencyKeyRequired() =>
        new("PayoutIdempotencyKey is required for retry-safe execution.");

    public static DomainException SharesRequired() =>
        new("Payout requires at least one participant share.");

    public static DomainException TotalMustBePositive() =>
        new("Total payout amount must be greater than zero.");

    public static DomainException CompensationNotAllowed(PayoutStatus status) =>
        new($"Payout compensation is only allowed from Executed or Failed state (was {status}).");

    public static DomainException AlreadyCompensated() =>
        new("Payout has already been compensated.");

    public static DomainException CompensationNotRequested() =>
        new("Payout cannot be marked Compensated without a prior CompensationRequested event.");

    public static DomainException CompensatingJournalIdRequired() =>
        new("CompensatingJournalId is required to mark a payout as Compensated.");

    public static DomainException RetryRequiresCompensatedOrFailed(PayoutStatus priorStatus) =>
        new($"Payout retry is only permitted when the prior attempt is Compensated or Failed (was {priorStatus}).");
}
