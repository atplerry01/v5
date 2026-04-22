namespace Whycespace.Shared.Contracts.Trust.Identity.Consent;

public sealed record ConsentReadModel
{
    public Guid ConsentId { get; init; }
    public Guid IdentityReference { get; init; }
    public string ConsentScope { get; init; } = string.Empty;
    public string ConsentPurpose { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset GrantedAt { get; init; }
    public DateTimeOffset LastUpdatedAt { get; init; }
}
