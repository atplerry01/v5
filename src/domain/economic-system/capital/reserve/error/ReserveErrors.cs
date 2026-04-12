using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Reserve;

public static class ReserveErrors
{
    public static DomainException ReserveAlreadyReleased() =>
        new("Reserve has already been released.");

    public static DomainException ReserveAlreadyExpired() =>
        new("Reserve has already expired.");

    public static DomainException InvalidAmount() =>
        new("Amount must be greater than zero.");

    public static DomainException InvalidCurrencyCode() =>
        new("Currency code is invalid. Must be a non-empty ISO currency code.");

    public static DomainException CannotReleaseExpiredReservation() =>
        new("Cannot release a reservation that has already expired.");

    public static DomainException CannotExpireReleasedReservation() =>
        new("Cannot expire a reservation that has already been released.");

    public static DomainException ReservationNotActive() =>
        new("Reservation is not in an active state.");

    public static DomainInvariantViolationException NegativeReserveAmount() =>
        new("Invariant violated: reserve amount cannot be negative.");

    public static DomainException ExpiryMustBeFuture() =>
        new("Expiry timestamp must be after the reservation timestamp.");
}
