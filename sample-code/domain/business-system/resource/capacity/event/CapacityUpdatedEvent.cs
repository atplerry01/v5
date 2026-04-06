using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.BusinessSystem.Resource.Capacity;

public sealed record CapacityUpdatedEvent(Guid CapacityId) : DomainEvent;
