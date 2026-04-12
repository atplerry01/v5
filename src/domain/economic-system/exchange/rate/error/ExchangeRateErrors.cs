using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Exchange.Rate;

public static class ExchangeRateErrors
{
    // ── Operation Errors ─────────────────────────────────────────

    public static DomainException InvalidRateValue(decimal value) =>
        new($"Rate value must be greater than zero. Received: {value}.");

    public static DomainException DuplicateActiveRate(Currency baseCurrency, Currency quoteCurrency) =>
        new($"An active rate already exists for pair '{baseCurrency.Code}/{quoteCurrency.Code}'.");

    public static DomainException InvalidStateTransition(ExchangeRateStatus current, ExchangeRateStatus target) =>
        new($"Cannot transition from '{current}' to '{target}'.");

    // ── Invariant Violations ─────────────────────────────────────

    public static DomainInvariantViolationException RateValueMustBePositive() =>
        new("Invariant violated: rate value must be greater than zero.");

    public static DomainInvariantViolationException VersionMustBePositive() =>
        new("Invariant violated: rate version must be greater than zero.");
}
