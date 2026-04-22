namespace Whycespace.Shared.Contracts.Platform.Routing.RouteResolution;

public sealed record RouteResolutionReadModel
{
    public Guid ResolutionId { get; init; }
    public string SourceClassification { get; init; } = string.Empty;
    public string SourceContext { get; init; } = string.Empty;
    public string SourceDomain { get; init; } = string.Empty;
    public string MessageType { get; init; } = string.Empty;
    public Guid? ResolvedRouteRef { get; init; }
    public string Strategy { get; init; } = string.Empty;
    public IReadOnlyList<Guid> DispatchRulesEvaluated { get; init; } = [];
    public string Outcome { get; init; } = string.Empty;
    public string? FailureReason { get; init; }
    public DateTimeOffset LastModifiedAt { get; init; }
}
