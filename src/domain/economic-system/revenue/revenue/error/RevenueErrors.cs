using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Revenue.Revenue;

public static class RevenueErrors
{
    public static DomainException InvalidAmount() =>
        new("Revenue amount must be greater than zero.");

    public static DomainException MissingContractReference() =>
        new("Revenue must originate from a contract.");

    public static DomainException RevenueNotRecognized() =>
        new("Revenue must be in Recognized status.");

    public static DomainException RevenueAlreadyDistributed() =>
        new("Revenue has already been distributed.");

    public static DomainInvariantViolationException NegativeRevenue() =>
        new("Invariant violated: revenue amount cannot be negative.");
}
