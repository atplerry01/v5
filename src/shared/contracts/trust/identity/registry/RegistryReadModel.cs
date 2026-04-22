namespace Whycespace.Shared.Contracts.Trust.Identity.Registry;

public sealed record RegistryReadModel
{
    public Guid RegistryId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string RegistrationType { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset InitiatedAt { get; init; }
    public DateTimeOffset LastUpdatedAt { get; init; }
    public string? RejectionReason { get; init; }
    public bool IsLocked { get; init; }
    public string? LockReason { get; init; }
}
