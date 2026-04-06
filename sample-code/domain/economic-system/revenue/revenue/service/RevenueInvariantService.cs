using Whycespace.Domain.EconomicSystem.Ledger.Obligation;

namespace Whycespace.Domain.EconomicSystem.Revenue.Revenue;

public sealed class RevenueInvariantService
{
    public RevenueInvariantResult Validate(
        Money settlementAmount,
        Money recognizedAmount,
        ObligationStatus obligationStatus)
    {
        if (settlementAmount.IsZero || settlementAmount.IsNegative)
            return RevenueInvariantResult.Fail("Invalid settlement amount");

        if (recognizedAmount.IsNegative)
            return RevenueInvariantResult.Fail("Negative revenue not allowed");

        if (recognizedAmount > settlementAmount)
            return RevenueInvariantResult.Fail("Cannot recognize more than settled");

        if (obligationStatus != ObligationStatus.Settled)
            return RevenueInvariantResult.Fail("Obligation not fulfilled");

        return RevenueInvariantResult.Success();
    }
}

public sealed record RevenueInvariantResult(
    bool IsValid,
    string? Error)
{
    public static RevenueInvariantResult Fail(string error)
        => new(false, error);

    public static RevenueInvariantResult Success()
        => new(true, null);
}
