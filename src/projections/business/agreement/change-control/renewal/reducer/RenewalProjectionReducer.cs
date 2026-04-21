using Whycespace.Shared.Contracts.Business.Agreement.ChangeControl.Renewal;
using Whycespace.Shared.Contracts.Events.Business.Agreement.ChangeControl.Renewal;

namespace Whycespace.Projections.Business.Agreement.ChangeControl.Renewal.Reducer;

public static class RenewalProjectionReducer
{
    public static RenewalReadModel Apply(RenewalReadModel state, RenewalCreatedEventSchema e) =>
        state with
        {
            RenewalId = e.AggregateId,
            SourceId = e.SourceId,
            Status = "Pending",
            CreatedAt = DateTimeOffset.MinValue,
            LastUpdatedAt = DateTimeOffset.MinValue
        };

    public static RenewalReadModel Apply(RenewalReadModel state, RenewalRenewedEventSchema e) =>
        state with
        {
            RenewalId = e.AggregateId,
            Status = "Renewed"
        };

    public static RenewalReadModel Apply(RenewalReadModel state, RenewalExpiredEventSchema e) =>
        state with
        {
            RenewalId = e.AggregateId,
            Status = "Expired"
        };
}
