namespace Whycespace.Domain.ConstitutionalSystem.Policy.Version;

using Whycespace.Domain.SharedKernel;

public sealed class EffectiveDateRange : ValueObject
{
    public DateTimeOffset Start { get; }
    public DateTimeOffset? End { get; }

    private EffectiveDateRange(DateTimeOffset start, DateTimeOffset? end)
    {
        if (end.HasValue && end.Value <= start)
            throw new ArgumentException("End date must be after start date.");

        Start = start;
        End = end;
    }

    public static EffectiveDateRange Create(DateTimeOffset start, DateTimeOffset? end = null)
        => new(start, end);

    public static EffectiveDateRange Indefinite(DateTimeOffset start)
        => new(start, null);

    public bool Contains(DateTimeOffset pointInTime)
    {
        if (pointInTime < Start) return false;
        if (End.HasValue && pointInTime >= End.Value) return false;
        return true;
    }

    public bool Overlaps(EffectiveDateRange other)
    {
        var thisEnd = End ?? DateTimeOffset.MaxValue;
        var otherEnd = other.End ?? DateTimeOffset.MaxValue;

        return Start < otherEnd && other.Start < thisEnd;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Start;
        yield return End ?? DateTimeOffset.MaxValue;
    }

    public override string ToString() =>
        End.HasValue ? $"{Start:O} → {End:O}" : $"{Start:O} → ∞";
}
