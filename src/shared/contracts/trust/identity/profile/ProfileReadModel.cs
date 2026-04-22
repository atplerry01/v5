namespace Whycespace.Shared.Contracts.Trust.Identity.Profile;

public sealed record ProfileReadModel
{
    public Guid ProfileId { get; init; }
    public Guid IdentityReference { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public string ProfileType { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset LastUpdatedAt { get; init; }
}
