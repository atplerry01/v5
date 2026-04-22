namespace Whycespace.Domain.ControlSystem.Audit.AuditQuery;

public readonly record struct QueryTimeRange
{
    public DateTimeOffset From { get; }
    public DateTimeOffset To { get; }

    public QueryTimeRange(DateTimeOffset from, DateTimeOffset to)
    {
        if (to <= from)
            throw AuditQueryErrors.QueryTimeRangeToMustBeAfterFrom();

        From = from;
        To = to;
    }

    public override string ToString() => $"{From:O}..{To:O}";
}
