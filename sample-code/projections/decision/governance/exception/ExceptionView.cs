namespace Whycespace.Projections.Decision.Governance.Exception;

public sealed record ExceptionView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
