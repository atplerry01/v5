using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.BusinessSystem.Resource.Reservation;

public sealed record ResourceAllocatedEvent(Guid AllocationId) : DomainEvent;
