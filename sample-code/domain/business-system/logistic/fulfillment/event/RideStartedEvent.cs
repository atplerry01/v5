using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.BusinessSystem.Logistic.Fulfillment;

public sealed record RideStartedEvent(Guid RideId) : DomainEvent;
