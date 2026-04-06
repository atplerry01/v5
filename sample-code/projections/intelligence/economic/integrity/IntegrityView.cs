namespace Whycespace.Projections.Intelligence.Economic.Integrity;

public sealed record IntegrityView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
