namespace Whycespace.Shared.Contracts.Events.Platform.Routing.RouteDescriptor;

public sealed record RouteDescriptorRegisteredEventSchema(
    Guid AggregateId,
    string SourceClassification,
    string SourceContext,
    string SourceDomain,
    string DestinationClassification,
    string DestinationContext,
    string DestinationDomain,
    string TransportHint,
    int Priority);
