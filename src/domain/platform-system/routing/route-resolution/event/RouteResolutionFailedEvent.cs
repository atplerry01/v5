using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Routing.RouteResolution;

public sealed record RouteResolutionFailedEvent(
    [property: JsonPropertyName("AggregateId")] ResolutionId ResolutionId,
    DomainRoute SourceRoute,
    string MessageType,
    IReadOnlyList<Guid> DispatchRulesEvaluated,
    string FailureReason,
    Timestamp ResolvedAt) : DomainEvent;
