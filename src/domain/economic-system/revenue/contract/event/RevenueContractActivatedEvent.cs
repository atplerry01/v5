using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Revenue.Contract;

public sealed record RevenueContractActivatedEvent(
    RevenueContractId ContractId,
    Timestamp ActivatedAt) : DomainEvent;
