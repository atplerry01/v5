using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Platform.Routing.RouteDescriptor;

public sealed record RegisterRouteDescriptorCommand(
    Guid RouteDescriptorId,
    string SourceClassification,
    string SourceContext,
    string SourceDomain,
    string DestinationClassification,
    string DestinationContext,
    string DestinationDomain,
    string TransportHint,
    int Priority,
    DateTimeOffset RegisteredAt) : IHasAggregateId
{
    public Guid AggregateId => RouteDescriptorId;
}
