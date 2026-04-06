namespace Whycespace.Projections.Decision.Compliance.ComplianceCase;

public sealed record ComplianceCaseView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
