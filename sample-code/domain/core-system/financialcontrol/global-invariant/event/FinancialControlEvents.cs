using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.CoreSystem.FinancialControl.GlobalInvariant;

public sealed record FinancialControlInitializedEvent(Guid ControlId, decimal InitialBalance) : DomainEvent;
public sealed record InflowRecordedEvent(Guid ControlId, decimal Amount, decimal NewTotalInflow) : DomainEvent;
public sealed record OutflowRecordedEvent(Guid ControlId, decimal Amount, decimal NewTotalOutflow) : DomainEvent;
public sealed record SystemBalanceViolationDetectedEvent(Guid ControlId, decimal Balance) : DomainEvent;
public sealed record VaultConsistencyVerifiedEvent(Guid ControlId, int VaultsVerified, bool IsConsistent, int DiscrepanciesFound) : DomainEvent;
public sealed record FinancialControlSealedEvent(Guid ControlId) : DomainEvent;
