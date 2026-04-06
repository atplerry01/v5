using Whycespace.Shared.Primitives.Id;
namespace Whycespace.Domain.OperationalSystem.IncidentResponse.Incident;

public readonly record struct IncidentId(Guid Value)
{
    public static IncidentId FromSeed(string seed) => new(DeterministicIdHelper.FromSeed(seed));
    public static readonly IncidentId Empty = new(Guid.Empty);

    public bool IsEmpty => Value == Guid.Empty;

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(IncidentId id) => id.Value;
    public static implicit operator IncidentId(Guid id) => new(id);
}
