using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.StructuralSystem.Contracts.References;

namespace Whycespace.Domain.StructuralSystem.Cluster.Administration;

public sealed record AdministrationAttachedEvent(
    [property: JsonPropertyName("AggregateId")] AdministrationId AdministrationId,
    ClusterRef ClusterRef,
    DateTimeOffset EffectiveAt) : DomainEvent;
