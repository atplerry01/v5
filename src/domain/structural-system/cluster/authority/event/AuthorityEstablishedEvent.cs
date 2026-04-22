using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Authority;

public sealed record AuthorityEstablishedEvent(
    [property: JsonPropertyName("AggregateId")] AuthorityId AuthorityId,
    AuthorityDescriptor Descriptor) : DomainEvent;
