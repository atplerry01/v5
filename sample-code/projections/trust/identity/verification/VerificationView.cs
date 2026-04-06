namespace Whycespace.Projections.Trust.Identity.Verification;

public sealed record VerificationView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
