using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Humancapital.Workforce;

public sealed record WorkforceCreatedEvent(
    [property: JsonPropertyName("AggregateId")] WorkforceId WorkforceId,
    WorkforceDescriptor Descriptor) : DomainEvent;
