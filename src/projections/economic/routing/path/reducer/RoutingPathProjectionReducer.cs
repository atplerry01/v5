using Whycespace.Shared.Contracts.Economic.Routing.Path;
using Whycespace.Shared.Contracts.Events.Economic.Routing.Path;

namespace Whycespace.Projections.Economic.Routing.Path.Reducer;

public static class RoutingPathProjectionReducer
{
    public static RoutingPathReadModel Apply(RoutingPathReadModel state, RoutingPathDefinedEventSchema e) =>
        state with
        {
            PathId = e.AggregateId,
            PathType = e.PathType,
            Conditions = e.Conditions,
            Priority = e.Priority,
            Status = "Defined"
        };

    public static RoutingPathReadModel Apply(RoutingPathReadModel state, RoutingPathActivatedEventSchema e) =>
        state with
        {
            PathId = e.AggregateId,
            Status = "Active",
            ActivatedAt = e.ActivatedAt
        };

    public static RoutingPathReadModel Apply(RoutingPathReadModel state, RoutingPathDisabledEventSchema e) =>
        state with
        {
            PathId = e.AggregateId,
            Status = "Disabled",
            DisabledAt = e.DisabledAt
        };
}
