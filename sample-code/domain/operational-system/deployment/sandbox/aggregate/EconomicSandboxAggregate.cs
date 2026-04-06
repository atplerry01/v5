using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.OperationalSystem.Deployment.Sandbox;

/// <summary>
/// Economic sandbox for controlled capital flow testing.
/// Enforces capital caps, transaction limits, and monitoring.
/// </summary>
public sealed class EconomicSandboxAggregate : AggregateRoot
{
    public string SandboxName { get; private set; } = string.Empty;
    public string RegionId { get; private set; } = string.Empty;
    public decimal CapitalCap { get; private set; }
    public decimal CurrentCapital { get; private set; }
    public int TransactionLimit { get; private set; }
    public int TransactionCount { get; private set; }
    public SandboxStatus Status { get; private set; } = SandboxStatus.Created;

    public static EconomicSandboxAggregate Create(
        Guid id, string sandboxName, string regionId, decimal capitalCap, int transactionLimit)
    {
        var agg = new EconomicSandboxAggregate
        {
            Id = id,
            SandboxName = sandboxName,
            RegionId = regionId,
            CapitalCap = capitalCap,
            TransactionLimit = transactionLimit
        };
        agg.RaiseDomainEvent(new SandboxCreatedEvent(id, sandboxName, regionId, capitalCap, transactionLimit));
        return agg;
    }

    public void Activate()
    {
        EnsureInvariant(Status == SandboxStatus.Created, "MustBeCreated",
            $"Cannot activate sandbox in {Status.Value} status.");
        Status = SandboxStatus.Active;
        RaiseDomainEvent(new SandboxActivatedEvent(Id));
    }

    public void RecordTransaction(decimal amount)
    {
        EnsureInvariant(Status == SandboxStatus.Active, "MustBeActive",
            "Sandbox must be active to record transactions.");
        EnsureInvariant(TransactionCount < TransactionLimit, "TransactionLimit",
            $"Transaction limit ({TransactionLimit}) reached.");
        EnsureInvariant(CurrentCapital + amount <= CapitalCap, "CapitalCap",
            $"Transaction of {amount} would exceed capital cap ({CapitalCap}).");

        CurrentCapital += amount;
        TransactionCount++;
        RaiseDomainEvent(new SandboxTransactionRecordedEvent(Id, amount, CurrentCapital, TransactionCount));
    }

    public void Close()
    {
        EnsureInvariant(Status == SandboxStatus.Active, "MustBeActive",
            "Only active sandboxes can be closed.");
        Status = SandboxStatus.Closed;
        RaiseDomainEvent(new SandboxClosedEvent(Id, CurrentCapital, TransactionCount));
    }
}
