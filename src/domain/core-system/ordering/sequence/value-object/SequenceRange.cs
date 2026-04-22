namespace Whycespace.Domain.CoreSystem.Ordering.Sequence;

public sealed record SequenceRange
{
    public Sequence Start { get; }
    public Sequence End { get; }

    public SequenceRange(Sequence start, Sequence end)
    {
        if (end < start)
            throw SequenceErrors.RangeEndMustFollowStart(start, end);

        Start = start;
        End = end;
    }

    public long Count => End.Value - Start.Value + 1;

    public bool Contains(Sequence sequence) => sequence >= Start && sequence <= End;
}
