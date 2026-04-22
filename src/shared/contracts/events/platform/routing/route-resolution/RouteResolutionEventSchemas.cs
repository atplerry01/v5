namespace Whycespace.Shared.Contracts.Events.Platform.Routing.RouteResolution;

public sealed record RouteResolvedEventSchema(
    Guid AggregateId,
    string SourceClassification,
    string SourceContext,
    string SourceDomain,
    string MessageType,
    Guid ResolvedRouteRef,
    string Strategy,
    IReadOnlyList<Guid> DispatchRulesEvaluated);

public sealed record RouteResolutionFailedEventSchema(
    Guid AggregateId,
    string SourceClassification,
    string SourceContext,
    string SourceDomain,
    string MessageType,
    IReadOnlyList<Guid> DispatchRulesEvaluated,
    string FailureReason);
