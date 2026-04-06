using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Capital;

public sealed record CapitalAllocatedEvent(Guid CapitalAccountId, Guid AllocationId) : DomainEvent;
