using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Administration;

public sealed record AdministrationEstablishedEvent(
    [property: JsonPropertyName("AggregateId")] AdministrationId AdministrationId,
    AdministrationDescriptor Descriptor) : DomainEvent;
