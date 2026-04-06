namespace Whycespace.Domain.SharedKernel.Primitives.Kernel;

/// <summary>
/// Identifies the aggregate instance that produced the event.
/// Used as the Kafka partition key for ordering guarantees.
/// </summary>
public readonly record struct AggregateId(Guid Value)
{
    public static readonly AggregateId Empty = new(Guid.Empty);

    public bool IsEmpty => Value == Guid.Empty;

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(AggregateId id) => id.Value;
    public static implicit operator AggregateId(Guid id) => new(id);
}