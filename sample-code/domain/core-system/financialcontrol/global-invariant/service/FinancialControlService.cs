namespace Whycespace.Domain.CoreSystem.FinancialControl.GlobalInvariant;

/// <summary>
/// Domain service for financial control invariant evaluation.
/// Stateless — all data passed as parameters.
/// </summary>
public sealed class FinancialControlService
{
    public bool IsBalanceHealthy(FinancialControlAggregate control) =>
        !control.Balance.IsNegative && control.FlowBalance.IsBalanced;

    public bool CanAcceptOutflow(FinancialControlAggregate control, decimal amount) =>
        !control.IsSealed && !control.Balance.Debit(amount).IsNegative;

    public bool IsVaultConsistent(FinancialControlAggregate control) =>
        control.LastVaultCheck is not null && control.LastVaultCheck.IsConsistent;
}
