namespace Whycespace.Projections.Decision.Compliance.Attestation;

public sealed record AttestationView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
