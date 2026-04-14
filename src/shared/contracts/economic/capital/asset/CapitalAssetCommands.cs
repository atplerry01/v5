namespace Whycespace.Shared.Contracts.Economic.Capital.Asset;

public sealed record CreateAssetCommand(
    Guid AssetId,
    Guid OwnerId,
    decimal InitialValue,
    string Currency,
    DateTimeOffset CreatedAt);

public sealed record RevalueAssetCommand(
    Guid AssetId,
    decimal NewValue,
    DateTimeOffset ValuedAt);

public sealed record DisposeAssetCommand(
    Guid AssetId,
    DateTimeOffset DisposedAt);
