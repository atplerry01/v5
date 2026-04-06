using Whycespace.Shared.Primitives.Id;
namespace Whycespace.Domain.IntelligenceSystem.Observability.Alert;

public readonly record struct AlertId(Guid Value)
{
    public static AlertId FromSeed(string seed) => new(DeterministicIdHelper.FromSeed(seed));
    public static readonly AlertId Empty = new(Guid.Empty);
    public bool IsEmpty => Value == Guid.Empty;
    public override string ToString() => Value.ToString();
    public static implicit operator Guid(AlertId id) => id.Value;
    public static implicit operator AlertId(Guid id) => new(id);
}
