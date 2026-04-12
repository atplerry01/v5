using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Revenue.Pricing;

public static class PricingErrors
{
    public static DomainException InvalidPrice() =>
        new("Price must be greater than zero.");

    public static DomainException MissingContractReference() =>
        new("Pricing must reference a contract.");

    public static DomainException MissingAdjustmentReason() =>
        new("Price adjustment must include a reason.");

    // ── Invariant Violations ─────────────────────────────────────

    public static DomainInvariantViolationException NegativePrice() =>
        new("Invariant violated: price cannot be negative.");

    public static DomainInvariantViolationException ContractReferenceMustExist() =>
        new("Invariant violated: pricing must reference a contract.");
}
