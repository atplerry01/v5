namespace Whycespace.Shared.Contracts.Platform.Routing.RouteDefinition;

public sealed record RouteDefinitionReadModel
{
    public Guid RouteDefinitionId { get; init; }
    public string RouteName { get; init; } = string.Empty;
    public string SourceClassification { get; init; } = string.Empty;
    public string SourceContext { get; init; } = string.Empty;
    public string SourceDomain { get; init; } = string.Empty;
    public string DestinationClassification { get; init; } = string.Empty;
    public string DestinationContext { get; init; } = string.Empty;
    public string DestinationDomain { get; init; } = string.Empty;
    public string TransportHint { get; init; } = string.Empty;
    public int Priority { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset LastModifiedAt { get; init; }
}
