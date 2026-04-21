using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PricingExecution.QuoteBasis;

public static class QuoteBasisErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("QuoteBasisId is required and must not be empty.");

    public static DomainException MissingPriceBookRef()
        => new DomainInvariantViolationException("QuoteBasis must reference a price-book.");

    public static DomainException InvalidStateTransition(QuoteBasisStatus currentStatus, string attemptedAction)
        => new DomainInvariantViolationException($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static DomainException FinalizedImmutable(QuoteBasisId id)
        => new DomainInvariantViolationException($"QuoteBasis '{id.Value}' is finalized and cannot be edited.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("QuoteBasis has already been initialized.");
}
