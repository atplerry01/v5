namespace Whycespace.Domain.SharedKernel.Primitive.Time;

/// <summary>
/// Default system clock implementation for the domain layer.
/// Used as fallback when no clock is injected via AggregateRoot.SetClock().
/// For deterministic replay, inject a DeterministicClock instead.
/// </summary>
public sealed class SystemClock : IClock
{
    public static readonly SystemClock Instance = new();

    public DateTime UtcNow => DateTime.UtcNow;
    public DateTimeOffset UtcNowOffset => DateTimeOffset.UtcNow;
}
