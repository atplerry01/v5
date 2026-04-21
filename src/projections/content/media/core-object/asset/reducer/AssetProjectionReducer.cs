using Whycespace.Shared.Contracts.Content.Media.CoreObject.Asset;
using Whycespace.Shared.Contracts.Events.Content.Media.CoreObject.Asset;

namespace Whycespace.Projections.Content.Media.CoreObject.Asset.Reducer;

public static class AssetProjectionReducer
{
    public static AssetReadModel Apply(AssetReadModel state, AssetCreatedEventSchema e) =>
        state with
        {
            AssetId = e.AggregateId,
            Title = e.Title,
            Classification = e.Classification,
            Kind = "Other",
            Status = "Draft",
            CreatedAt = e.CreatedAt,
            LastModifiedAt = e.CreatedAt
        };

    public static AssetReadModel Apply(AssetReadModel state, AssetRenamedEventSchema e) =>
        state with { AssetId = e.AggregateId, Title = e.NewTitle, LastModifiedAt = e.RenamedAt };

    public static AssetReadModel Apply(AssetReadModel state, AssetReclassifiedEventSchema e) =>
        state with { AssetId = e.AggregateId, Classification = e.NewClassification, LastModifiedAt = e.ReclassifiedAt };

    public static AssetReadModel Apply(AssetReadModel state, AssetActivatedEventSchema e) =>
        state with { AssetId = e.AggregateId, Status = "Active", LastModifiedAt = e.ActivatedAt };

    public static AssetReadModel Apply(AssetReadModel state, AssetRetiredEventSchema e) =>
        state with { AssetId = e.AggregateId, Status = "Retired", LastModifiedAt = e.RetiredAt };

    public static AssetReadModel Apply(AssetReadModel state, AssetKindAssignedEventSchema e) =>
        state with { AssetId = e.AggregateId, Kind = e.NewKind, LastModifiedAt = e.AssignedAt };
}
