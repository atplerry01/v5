namespace Whycespace.Domain.CoreSystem.FinancialControl.GlobalInvariant;

/// <summary>
/// Specification: total inflow must equal total outflow.
/// Pure predicate — no side effects.
/// </summary>
public sealed class InflowOutflowBalanceSpecification
{
    public bool IsSatisfiedBy(InflowOutflowBalance balance) => balance.IsBalanced;
}
