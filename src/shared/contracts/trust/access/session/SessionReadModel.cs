namespace Whycespace.Shared.Contracts.Trust.Access.Session;

public sealed record SessionReadModel
{
    public Guid SessionId { get; init; }
    public Guid IdentityReference { get; init; }
    public string SessionContext { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset OpenedAt { get; init; }
    public DateTimeOffset LastUpdatedAt { get; init; }
}
