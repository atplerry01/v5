using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.CoreSystem.FinancialControl.GlobalInvariant;

public sealed class NegativeSystemBalanceException : DomainException
{
    public NegativeSystemBalanceException(decimal balance)
        : base("NEGATIVE_SYSTEM_BALANCE", $"System balance is negative: {balance}. This violates the global financial invariant.") { }
}

public sealed class InflowOutflowImbalanceException : DomainException
{
    public InflowOutflowImbalanceException(decimal inflow, decimal outflow)
        : base("INFLOW_OUTFLOW_IMBALANCE", $"Total inflow ({inflow}) does not equal total outflow ({outflow}).") { }
}

public sealed class VaultInconsistencyException : DomainException
{
    public VaultInconsistencyException(int discrepancies)
        : base("VAULT_INCONSISTENCY", $"Vault consistency check failed with {discrepancies} discrepancies.") { }
}

public sealed class FinancialControlSealedException : DomainException
{
    public FinancialControlSealedException()
        : base("FINANCIAL_CONTROL_SEALED", "Financial control is sealed and cannot accept further operations.") { }
}
