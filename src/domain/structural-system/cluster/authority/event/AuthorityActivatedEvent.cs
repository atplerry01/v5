using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Authority;

public sealed record AuthorityActivatedEvent(
    [property: JsonPropertyName("AggregateId")] AuthorityId AuthorityId) : DomainEvent;
