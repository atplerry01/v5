using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.CoreSystem.FinancialControl.GlobalInvariant;

/// <summary>
/// Global financial invariant authority. Enforces:
/// - Total inflow = total outflow (at settlement)
/// - No negative system balance
/// - Vault consistency across the system
/// </summary>
public sealed class FinancialControlAggregate : AggregateRoot
{
    public InflowOutflowBalance FlowBalance { get; private set; } = InflowOutflowBalance.Initial();
    public SystemBalance Balance { get; private set; } = SystemBalance.Initial();
    public VaultConsistencyStatus? LastVaultCheck { get; private set; }
    public bool IsSealed { get; private set; }

    public static FinancialControlAggregate Initialize(Guid id, decimal initialBalance)
    {
        var agg = new FinancialControlAggregate
        {
            Id = id,
            Balance = SystemBalance.From(initialBalance),
            IsSealed = false
        };
        agg.RaiseDomainEvent(new FinancialControlInitializedEvent(id, initialBalance));
        return agg;
    }

    public void RecordInflow(decimal amount)
    {
        EnsureNotSealed();
        FlowBalance = FlowBalance.RecordInflow(amount);
        Balance = Balance.Credit(amount);
        RaiseDomainEvent(new InflowRecordedEvent(Id, amount, FlowBalance.TotalInflow));
    }

    public void RecordOutflow(decimal amount)
    {
        EnsureNotSealed();
        var projected = Balance.Debit(amount);
        EnsureInvariant(
            !projected.IsNegative,
            "NoNegativeBalance",
            $"Outflow of {amount} would result in negative system balance: {projected.Value}");
        FlowBalance = FlowBalance.RecordOutflow(amount);
        Balance = projected;
        RaiseDomainEvent(new OutflowRecordedEvent(Id, amount, FlowBalance.TotalOutflow));
    }

    public void DetectBalanceViolation()
    {
        if (Balance.IsNegative)
        {
            RaiseDomainEvent(new SystemBalanceViolationDetectedEvent(Id, Balance.Value));
        }
    }

    public void RecordVaultConsistencyCheck(int vaultsVerified, int discrepanciesFound)
    {
        EnsureNotSealed();
        LastVaultCheck = discrepanciesFound == 0
            ? VaultConsistencyStatus.Consistent(vaultsVerified)
            : VaultConsistencyStatus.Inconsistent(vaultsVerified, discrepanciesFound);
        RaiseDomainEvent(new VaultConsistencyVerifiedEvent(
            Id, vaultsVerified, LastVaultCheck.IsConsistent, discrepanciesFound));
    }

    public void Seal()
    {
        EnsureNotSealed();
        EnsureInvariant(
            FlowBalance.IsBalanced,
            "InflowEqualsOutflow",
            $"Cannot seal: inflow ({FlowBalance.TotalInflow}) != outflow ({FlowBalance.TotalOutflow})");
        EnsureInvariant(
            !Balance.IsNegative,
            "NoNegativeBalance",
            $"Cannot seal: system balance is negative ({Balance.Value})");
        IsSealed = true;
        RaiseDomainEvent(new FinancialControlSealedEvent(Id));
    }

    private void EnsureNotSealed()
    {
        if (IsSealed)
            throw new FinancialControlSealedException();
    }
}
