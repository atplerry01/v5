namespace Whycespace.Projections.Decision.Risk.Exception;

public sealed record ExceptionView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
