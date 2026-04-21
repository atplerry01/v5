using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Administration;

public sealed record AdministrationActivatedEvent(
    [property: JsonPropertyName("AggregateId")] AdministrationId AdministrationId) : DomainEvent;
