using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.BusinessSystem.Resource.Capacity;

public sealed record CapacityCreatedEvent(Guid CapacityId) : DomainEvent;
