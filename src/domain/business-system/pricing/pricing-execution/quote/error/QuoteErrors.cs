using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Pricing.PricingExecution.Quote;

public static class QuoteErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("QuoteId is required and must not be empty.");

    public static DomainException MissingQuoteBasisRef()
        => new DomainInvariantViolationException("Quote must reference a quote-basis.");

    public static DomainException InvalidStateTransition(QuoteStatus currentStatus, string attemptedAction)
        => new DomainInvariantViolationException($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static DomainException IssuanceRequiresValidity()
        => new DomainInvariantViolationException("Quote issuance requires a validity window.");

    public static DomainException AlreadyTerminal(QuoteId id, QuoteStatus status)
        => new DomainInvariantViolationException($"Quote '{id.Value}' is already terminal ({status}) and cannot be acted on.");

    public static DomainException ExpiryNotReached()
        => new DomainInvariantViolationException("Quote cannot be marked expired before its validity end.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("Quote has already been initialized.");
}
