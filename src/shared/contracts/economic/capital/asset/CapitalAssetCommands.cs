using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Economic.Capital.Asset;

public sealed record CreateAssetCommand(
    Guid AssetId,
    Guid OwnerId,
    decimal InitialValue,
    string Currency,
    DateTimeOffset CreatedAt) : IHasAggregateId
{
    public Guid AggregateId => AssetId;
}

public sealed record RevalueAssetCommand(
    Guid AssetId,
    decimal NewValue,
    DateTimeOffset ValuedAt) : IHasAggregateId
{
    public Guid AggregateId => AssetId;
}

public sealed record DisposeAssetCommand(
    Guid AssetId,
    DateTimeOffset DisposedAt) : IHasAggregateId
{
    public Guid AggregateId => AssetId;
}
