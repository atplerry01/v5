using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Subcluster;

public sealed record SubclusterDefinedEvent(
    [property: JsonPropertyName("AggregateId")] SubclusterId SubclusterId,
    SubclusterDescriptor Descriptor) : DomainEvent;
