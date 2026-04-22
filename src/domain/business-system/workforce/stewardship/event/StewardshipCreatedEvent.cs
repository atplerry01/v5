using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Workforce.Stewardship;

public sealed record StewardshipCreatedEvent(
    [property: JsonPropertyName("AggregateId")] StewardshipId StewardshipId,
    StewardshipDescriptor Descriptor) : DomainEvent;
