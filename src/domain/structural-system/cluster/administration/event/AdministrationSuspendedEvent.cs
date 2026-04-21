using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Administration;

public sealed record AdministrationSuspendedEvent(
    [property: JsonPropertyName("AggregateId")] AdministrationId AdministrationId) : DomainEvent;
