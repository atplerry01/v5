using Whycespace.Domain.SharedKernel;
using Whycespace.Domain.BusinessSystem.Logistic;

namespace Whycespace.Domain.BusinessSystem.Logistic.Fulfillment;

public sealed class RideAggregate : AggregateRoot
{
    public RideId RideId { get; private set; } = default!;
    public Guid RiderIdentityId { get; private set; }
    public Guid DriverIdentityId { get; private set; }
    public RideStatus Status { get; private set; } = RideStatus.Requested;
    public FareAmount Fare { get; private set; } = FareAmount.Zero;
    public Location StartLocation { get; private set; } = Location.Unknown;
    public Location EndLocation { get; private set; } = Location.Unknown;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    public static RideAggregate RequestRide(
        Guid riderIdentityId,
        Location startLocation,
        Location endLocation)
    {
        Guard.AgainstDefault(riderIdentityId);
        Guard.AgainstNull(startLocation);
        Guard.AgainstNull(endLocation);
        Guard.AgainstInvalid(startLocation, l => l != Location.Unknown, "Start location is required.");
        Guard.AgainstInvalid(endLocation, l => l != Location.Unknown, "End location is required.");

        var ride = new RideAggregate();
        var rideId = RideId.FromSeed($"Ride:{riderIdentityId}:{startLocation.Latitude}:{startLocation.Longitude}");

        ride.Apply(new RideRequestedEvent(
            rideId.Value,
            riderIdentityId,
            startLocation.Latitude,
            startLocation.Longitude,
            startLocation.Label,
            endLocation.Latitude,
            endLocation.Longitude,
            endLocation.Label));

        return ride;
    }

    public void AcceptRide(Guid driverIdentityId)
    {
        Guard.AgainstDefault(driverIdentityId);

        if (Status.IsTerminal)
            throw new DomainException(RideErrors.InvalidTransition,
                $"Cannot accept a ride in '{Status.Value}' status.");

        if (Status != RideStatus.Requested)
            throw new DomainException(RideErrors.RideAlreadyAccepted,
                "Ride has already been accepted.");

        Apply(new RideAcceptedEvent(Id, driverIdentityId));
    }

    public void StartRide()
    {
        if (Status != RideStatus.Accepted)
            throw new DomainException(RideErrors.RideNotAccepted,
                "Ride must be accepted before it can be started.");

        Apply(new RideStartedEvent(Id));
    }

    public void CompleteRide(FareAmount fare)
    {
        Guard.AgainstNull(fare);

        if (Status == RideStatus.Completed)
            throw new DomainException(RideErrors.AlreadyCompleted,
                "Ride is already completed.");

        if (Status != RideStatus.InProgress)
            throw new DomainException(RideErrors.NotInProgress,
                "Ride must be in progress to be completed.");

        Apply(new RideCompletedEvent(Id, fare.Value, fare.Currency));
    }

    public void CancelRide(string reason)
    {
        Guard.AgainstEmpty(reason);

        if (Status == RideStatus.Cancelled)
            throw new DomainException(RideErrors.AlreadyCancelled,
                "Ride is already cancelled.");

        if (Status.IsTerminal)
            throw new DomainException(RideErrors.InvalidTransition,
                $"Cannot cancel a ride in '{Status.Value}' status.");

        Apply(new RideCancelledEvent(Id, reason));
    }

    private void Apply(RideRequestedEvent e)
    {
        Id = e.RideId;
        RideId = new RideId(e.RideId);
        RiderIdentityId = e.RiderIdentityId;
        Status = RideStatus.Requested;
        StartLocation = new Location(e.StartLatitude, e.StartLongitude, e.StartLabel);
        EndLocation = new Location(e.EndLatitude, e.EndLongitude, e.EndLabel);
        CreatedAt = e.OccurredAt;
        RaiseDomainEvent(e);
    }

    private void Apply(RideAcceptedEvent e)
    {
        DriverIdentityId = e.DriverIdentityId;
        Status = RideStatus.Accepted;
        RaiseDomainEvent(e);
    }

    private void Apply(RideStartedEvent e)
    {
        Status = RideStatus.InProgress;
        RaiseDomainEvent(e);
    }

    private void Apply(RideCompletedEvent e)
    {
        Status = RideStatus.Completed;
        Fare = FareAmount.Of(e.FareAmount, e.FareCurrency);
        CompletedAt = e.OccurredAt;
        RaiseDomainEvent(e);
    }

    private void Apply(RideCancelledEvent e)
    {
        Status = RideStatus.Cancelled;
        RaiseDomainEvent(e);
    }
}
