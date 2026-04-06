namespace Whycespace.Shared.Primitives.Time;

public sealed record Timestamp
{
    public DateTime Value { get; }

    public Timestamp(DateTime value)
    {
        Value = value.Kind == DateTimeKind.Utc
            ? value
            : value.ToUniversalTime();
    }

    public static Timestamp Now(IClock clock) => new(clock.UtcNow);

    public override string ToString() => Value.ToString("O");
}
