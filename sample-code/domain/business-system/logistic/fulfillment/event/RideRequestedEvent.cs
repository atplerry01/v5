using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.BusinessSystem.Logistic.Fulfillment;

public sealed record RideRequestedEvent(
    Guid RideId,
    Guid RiderIdentityId,
    double StartLatitude,
    double StartLongitude,
    string StartLabel,
    double EndLatitude,
    double EndLongitude,
    string EndLabel
) : DomainEvent;
