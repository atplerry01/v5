using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.BusinessSystem.Resource.Utilization;

public sealed record ResourceCapacityReducedEvent(
    Guid ResourceId,
    decimal ReducedCapacity,
    decimal NewTotalCapacity,
    decimal NewAvailableCapacity
) : DomainEvent;
