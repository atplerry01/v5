namespace Whycespace.Projections.Business.Document.Retention;

public sealed record RetentionView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
