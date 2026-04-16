using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Monetization.Pricing;

public static class PricingErrors
{
    public static DomainException InvalidAmount() => new("Pricing amount must be non-negative.");
    public static DomainException InvalidCurrency() => new("Currency must be a 3-letter ISO code.");
    public static DomainException InvalidName() => new("Pricing plan name must be non-empty.");
    public static DomainException AlreadyPublished() => new("Pricing plan is already published.");
    public static DomainException AlreadyDeprecated() => new("Pricing plan is already deprecated.");
    public static DomainException CannotMutateDeprecated() => new("Deprecated pricing plans cannot be mutated.");
    public static DomainException CannotPublishWithoutPrice() => new("Pricing plan requires a published price to be published.");
    public static DomainInvariantViolationException NameMissing() =>
        new("Invariant violated: pricing plan must have a name.");
}
