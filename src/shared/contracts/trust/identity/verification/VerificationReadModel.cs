namespace Whycespace.Shared.Contracts.Trust.Identity.Verification;

public sealed record VerificationReadModel
{
    public Guid VerificationId { get; init; }
    public Guid IdentityReference { get; init; }
    public string ClaimType { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset InitiatedAt { get; init; }
    public DateTimeOffset LastUpdatedAt { get; init; }
}
