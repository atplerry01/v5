using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Exchange.Fx;

public static class FxErrors
{
    // ── Operation Errors ─────────────────────────────────────────

    public static DomainException MissingId() =>
        new("FxId is required and must not be empty.");

    public static DomainException MissingCurrencyPair() =>
        new("FX pair must define a valid currency pair.");

    public static DomainException InvalidStateTransition(FxStatus current, FxStatus target) =>
        new($"Cannot transition from '{current}' to '{target}'.");

    // ── Invariant Violations ─────────────────────────────────────

    public static DomainInvariantViolationException IdMustNotBeEmpty() =>
        new("Invariant violated: FxId must not be empty.");

    public static DomainInvariantViolationException CurrencyPairMustBeDefined() =>
        new("Invariant violated: currency pair must be defined.");
}
