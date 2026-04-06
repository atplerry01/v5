using Whycespace.Shared.Primitives.Id;
namespace Whycespace.Domain.BusinessSystem.Resource.Reservation;

public readonly record struct ReservationId(Guid Value)
{
    public static ReservationId FromSeed(string seed) => new(DeterministicIdHelper.FromSeed(seed));
    public static readonly ReservationId Empty = new(Guid.Empty);

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(ReservationId id) => id.Value;
    public static implicit operator ReservationId(Guid id) => new(id);
}
