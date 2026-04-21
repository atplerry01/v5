namespace Whycespace.Shared.Contracts.Content.Document.CoreObject.Bundle;

public sealed record DocumentBundleReadModel
{
    public Guid BundleId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset LastModifiedAt { get; init; }
    public DateTimeOffset? FinalizedAt { get; init; }
}
