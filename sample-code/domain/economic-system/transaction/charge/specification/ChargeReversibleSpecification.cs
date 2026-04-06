namespace Whycespace.Domain.EconomicSystem.Transaction.Charge;

/// <summary>
/// Specification that determines whether a charge can be reversed.
/// A charge is reversible only when its status is Applied.
/// </summary>
public static class ChargeReversibleSpecification
{
    public static bool IsSatisfiedBy(ChargeAggregate charge)
    {
        return charge.Status == ChargeStatus.Applied;
    }
}
