using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Revenue.Contract;

public sealed record RevenueContractTerminatedEvent(
    RevenueContractId ContractId,
    string Reason,
    Timestamp TerminatedAt) : DomainEvent;
