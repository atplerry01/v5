using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.OperationalSystem.Deployment.Sandbox;

public sealed record SandboxCreatedEvent(Guid SandboxId, string Name, string RegionId, decimal CapitalCap, int TransactionLimit) : DomainEvent;
public sealed record SandboxActivatedEvent(Guid SandboxId) : DomainEvent;
public sealed record SandboxTransactionRecordedEvent(Guid SandboxId, decimal Amount, decimal RunningTotal, int TransactionCount) : DomainEvent;
public sealed record SandboxClosedEvent(Guid SandboxId, decimal FinalCapital, int FinalTransactionCount) : DomainEvent;
