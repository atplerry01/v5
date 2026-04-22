namespace Whycespace.Shared.Contracts.Control.AccessControl.Identity;

public sealed record IdentityReadModel
{
    public Guid IdentityId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Kind { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string? SuspensionReason { get; init; }
}
