using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Content.Media.CoreObject.Asset;

public sealed record CreateAssetCommand(
    Guid AssetId,
    string Title,
    string Classification,
    DateTimeOffset CreatedAt) : IHasAggregateId
{
    public Guid AggregateId => AssetId;
}

public sealed record RenameAssetCommand(
    Guid AssetId,
    string NewTitle,
    DateTimeOffset RenamedAt) : IHasAggregateId
{
    public Guid AggregateId => AssetId;
}

public sealed record ReclassifyAssetCommand(
    Guid AssetId,
    string NewClassification,
    DateTimeOffset ReclassifiedAt) : IHasAggregateId
{
    public Guid AggregateId => AssetId;
}

public sealed record ActivateAssetCommand(
    Guid AssetId,
    DateTimeOffset ActivatedAt) : IHasAggregateId
{
    public Guid AggregateId => AssetId;
}

public sealed record RetireAssetCommand(
    Guid AssetId,
    DateTimeOffset RetiredAt) : IHasAggregateId
{
    public Guid AggregateId => AssetId;
}

public sealed record AssignAssetKindCommand(
    Guid AssetId,
    string NewKind,
    DateTimeOffset AssignedAt) : IHasAggregateId
{
    public Guid AggregateId => AssetId;
}
