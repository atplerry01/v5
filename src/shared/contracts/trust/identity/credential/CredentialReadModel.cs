namespace Whycespace.Shared.Contracts.Trust.Identity.Credential;

public sealed record CredentialReadModel
{
    public Guid CredentialId { get; init; }
    public Guid IdentityReference { get; init; }
    public string CredentialType { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset IssuedAt { get; init; }
    public DateTimeOffset LastUpdatedAt { get; init; }
}
