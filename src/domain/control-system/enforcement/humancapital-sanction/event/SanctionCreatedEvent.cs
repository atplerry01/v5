using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.Enforcement.HumancapitalSanction;

public sealed record SanctionCreatedEvent(
    [property: JsonPropertyName("AggregateId")] SanctionId SanctionId,
    SanctionDescriptor Descriptor) : DomainEvent;
