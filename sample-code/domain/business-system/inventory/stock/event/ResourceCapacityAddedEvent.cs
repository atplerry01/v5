using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.BusinessSystem.Resource.Utilization;

public sealed record ResourceCapacityAddedEvent(
    Guid ResourceId,
    decimal AddedCapacity,
    decimal NewTotalCapacity,
    decimal NewAvailableCapacity
) : DomainEvent;
