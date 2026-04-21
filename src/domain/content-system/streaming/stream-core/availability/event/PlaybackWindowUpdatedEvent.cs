using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.Availability;

public sealed record PlaybackWindowUpdatedEvent(
    [property: JsonPropertyName("AggregateId")] PlaybackId PlaybackId,
    PlaybackWindow PreviousWindow,
    PlaybackWindow NewWindow,
    Timestamp UpdatedAt) : DomainEvent;
