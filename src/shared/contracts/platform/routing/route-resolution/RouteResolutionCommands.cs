using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Platform.Routing.RouteResolution;

public sealed record ResolveRouteCommand(
    Guid ResolutionId,
    string SourceClassification,
    string SourceContext,
    string SourceDomain,
    string MessageType,
    Guid ResolvedRouteRef,
    string ResolutionStrategy,
    IReadOnlyList<Guid> DispatchRulesEvaluated,
    DateTimeOffset ResolvedAt) : IHasAggregateId
{
    public Guid AggregateId => ResolutionId;
}

public sealed record FailRouteResolutionCommand(
    Guid ResolutionId,
    string SourceClassification,
    string SourceContext,
    string SourceDomain,
    string MessageType,
    IReadOnlyList<Guid> DispatchRulesEvaluated,
    string FailureReason,
    DateTimeOffset ResolvedAt) : IHasAggregateId
{
    public Guid AggregateId => ResolutionId;
}
