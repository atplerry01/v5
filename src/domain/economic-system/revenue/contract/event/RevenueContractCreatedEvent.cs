using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Revenue.Contract;

public sealed record RevenueContractCreatedEvent(
    RevenueContractId ContractId,
    IReadOnlyList<RevenueShareRule> RevenueShareRules,
    ContractTerm Term,
    Timestamp CreatedAt) : DomainEvent;
