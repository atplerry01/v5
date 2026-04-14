using Whycespace.Shared.Contracts.Economic.Capital.Asset;
using Whycespace.Shared.Contracts.Events.Economic.Capital.Asset;

namespace Whycespace.Projections.Economic.Capital.Asset.Reducer;

public static class CapitalAssetProjectionReducer
{
    public static CapitalAssetReadModel Apply(CapitalAssetReadModel state, AssetCreatedEventSchema e) =>
        state with
        {
            AssetId = e.AggregateId,
            OwnerId = e.OwnerId,
            Value = e.InitialValue,
            Currency = e.Currency,
            Status = "Active",
            CreatedAt = e.CreatedAt,
            LastValuedAt = e.CreatedAt
        };

    public static CapitalAssetReadModel Apply(CapitalAssetReadModel state, AssetValuedEventSchema e) =>
        state with
        {
            AssetId = e.AggregateId,
            Value = e.NewValue,
            Currency = e.Currency,
            Status = "Valued",
            LastValuedAt = e.ValuedAt
        };

    public static CapitalAssetReadModel Apply(CapitalAssetReadModel state, AssetDisposedEventSchema e) =>
        state with
        {
            AssetId = e.AggregateId,
            Status = "Disposed"
        };
}
