using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Order.OrderCore.Reservation;

public static class ReservationErrors
{
    public static DomainException MissingId()
        => new DomainInvariantViolationException("ReservationId is required and must not be empty.");

    public static DomainException MissingOrderRef()
        => new DomainInvariantViolationException("Reservation must reference an order.");

    public static DomainException MissingSubject()
        => new DomainInvariantViolationException("Reservation must declare a subject reference.");

    public static DomainException MissingQuantity()
        => new DomainInvariantViolationException("Reservation must declare a positive quantity.");

    public static DomainException MissingExpiry()
        => new DomainInvariantViolationException("Reservation must declare an expiry.");

    public static DomainException InvalidStateTransition(ReservationStatus currentStatus, string attemptedAction)
        => new DomainInvariantViolationException($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static DomainException AlreadyTerminal(ReservationId id, ReservationStatus status)
        => new DomainInvariantViolationException($"Reservation '{id.Value}' is already terminal ({status}).");

    public static DomainException ExpiryInPast()
        => new DomainInvariantViolationException("ReservationExpiry cannot already be in the past at the moment of creation.");

    public static DomainException ExpiryNotReached()
        => new DomainInvariantViolationException("Reservation cannot be marked expired before its expiry moment.");

    public static DomainException AlreadyInitialized()
        => new DomainInvariantViolationException("Reservation has already been initialized.");
}
