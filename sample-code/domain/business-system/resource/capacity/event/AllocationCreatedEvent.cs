using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.BusinessSystem.Resource.Reservation;

public sealed record AllocationCreatedEvent(Guid AllocationId) : DomainEvent;
