namespace Whycespace.Domain.IntelligenceSystem.Observability.Health;

public readonly record struct ComponentId(string Value)
{
    public static ComponentId New(string value) => new(value);
    public static readonly ComponentId Empty = new(string.Empty);

    public bool IsEmpty => string.IsNullOrEmpty(Value);

    public override string ToString() => Value;

    public static implicit operator string(ComponentId id) => id.Value;
    public static implicit operator ComponentId(string id) => new(id);
}
