using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.BusinessSystem.Logistic.Fulfillment;

public sealed record RideCompletedEvent(
    Guid RideId,
    decimal FareAmount,
    string FareCurrency
) : DomainEvent;
