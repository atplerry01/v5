using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.StructuralSystem.Contracts.References;

namespace Whycespace.Domain.StructuralSystem.Cluster.Administration;

public sealed record AdministrationBindingValidatedEvent(
    [property: JsonPropertyName("AggregateId")] AdministrationId AdministrationId,
    ClusterRef Parent,
    StructuralParentState ParentState,
    DateTimeOffset EffectiveAt) : DomainEvent;
