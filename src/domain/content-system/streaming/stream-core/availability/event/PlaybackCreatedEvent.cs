using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.Availability;

public sealed record PlaybackCreatedEvent(
    [property: JsonPropertyName("AggregateId")] PlaybackId PlaybackId,
    PlaybackSourceRef SourceRef,
    PlaybackMode Mode,
    PlaybackWindow Window,
    Timestamp CreatedAt) : DomainEvent;
