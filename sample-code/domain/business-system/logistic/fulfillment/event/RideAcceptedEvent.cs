using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.BusinessSystem.Logistic.Fulfillment;

public sealed record RideAcceptedEvent(Guid RideId, Guid DriverIdentityId) : DomainEvent;
