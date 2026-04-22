using Whycespace.Shared.Contracts.Events.Platform.Routing.RouteResolution;
using Whycespace.Shared.Contracts.Platform.Routing.RouteResolution;

namespace Whycespace.Projections.Platform.Routing.RouteResolution.Reducer;

public static class RouteResolutionProjectionReducer
{
    public static RouteResolutionReadModel Apply(RouteResolutionReadModel state, RouteResolvedEventSchema e, DateTimeOffset at) =>
        state with
        {
            ResolutionId = e.AggregateId,
            SourceClassification = e.SourceClassification,
            SourceContext = e.SourceContext,
            SourceDomain = e.SourceDomain,
            MessageType = e.MessageType,
            ResolvedRouteRef = e.ResolvedRouteRef,
            Strategy = e.Strategy,
            DispatchRulesEvaluated = e.DispatchRulesEvaluated,
            Outcome = "Resolved",
            LastModifiedAt = at
        };

    public static RouteResolutionReadModel Apply(RouteResolutionReadModel state, RouteResolutionFailedEventSchema e, DateTimeOffset at) =>
        state with
        {
            ResolutionId = e.AggregateId,
            SourceClassification = e.SourceClassification,
            SourceContext = e.SourceContext,
            SourceDomain = e.SourceDomain,
            MessageType = e.MessageType,
            DispatchRulesEvaluated = e.DispatchRulesEvaluated,
            Outcome = "Failed",
            FailureReason = e.FailureReason,
            LastModifiedAt = at
        };
}
