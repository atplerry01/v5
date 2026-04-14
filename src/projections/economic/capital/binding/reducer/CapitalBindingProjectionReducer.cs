using Whycespace.Shared.Contracts.Economic.Capital.Binding;
using Whycespace.Shared.Contracts.Events.Economic.Capital.Binding;

namespace Whycespace.Projections.Economic.Capital.Binding.Reducer;

public static class CapitalBindingProjectionReducer
{
    public static CapitalBindingReadModel Apply(CapitalBindingReadModel state, CapitalBoundEventSchema e) =>
        state with
        {
            BindingId = e.AggregateId,
            AccountId = e.AccountId,
            OwnerId = e.OwnerId,
            OwnershipType = e.OwnershipType,
            Status = "Active",
            BoundAt = e.BoundAt
        };

    public static CapitalBindingReadModel Apply(CapitalBindingReadModel state, OwnershipTransferredEventSchema e) =>
        state with
        {
            BindingId = e.AggregateId,
            OwnerId = e.NewOwnerId,
            OwnershipType = e.NewOwnershipType,
            Status = "Transferred"
        };

    public static CapitalBindingReadModel Apply(CapitalBindingReadModel state, BindingReleasedEventSchema e) =>
        state with
        {
            BindingId = e.AggregateId,
            Status = "Released"
        };
}
