using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.BusinessSystem.Logistic.Fulfillment;

public sealed record RideCancelledEvent(Guid RideId, string Reason) : DomainEvent;
