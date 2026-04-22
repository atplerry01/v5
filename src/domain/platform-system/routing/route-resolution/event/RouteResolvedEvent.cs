using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Routing.RouteResolution;

public sealed record RouteResolvedEvent(
    [property: JsonPropertyName("AggregateId")] ResolutionId ResolutionId,
    DomainRoute SourceRoute,
    string MessageType,
    Guid ResolvedRouteRef,
    ResolutionStrategy Strategy,
    IReadOnlyList<Guid> DispatchRulesEvaluated,
    Timestamp ResolvedAt) : DomainEvent;
