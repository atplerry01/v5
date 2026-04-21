using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Event.EventStream;

public sealed record EventStreamOpenedEvent(
    [property: JsonPropertyName("AggregateId")] EventStreamId EventStreamId,
    StreamDescriptor Descriptor) : DomainEvent;
