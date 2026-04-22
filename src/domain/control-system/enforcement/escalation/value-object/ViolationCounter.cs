namespace Whycespace.Domain.ControlSystem.Enforcement.Escalation;

public readonly record struct ViolationCounter
{
    public int Count { get; }
    public int SeverityScore { get; }

    public ViolationCounter(int count, int severityScore)
    {
        if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
        if (severityScore < 0) throw new ArgumentOutOfRangeException(nameof(severityScore));
        Count = count;
        SeverityScore = severityScore;
    }

    public static readonly ViolationCounter Zero = new(0, 0);

    public ViolationCounter Add(int severityWeight) =>
        new(Count + 1, SeverityScore + severityWeight);
}
