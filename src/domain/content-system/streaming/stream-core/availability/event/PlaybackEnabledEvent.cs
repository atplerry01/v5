using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.Availability;

public sealed record PlaybackEnabledEvent(
    [property: JsonPropertyName("AggregateId")] PlaybackId PlaybackId,
    Timestamp EnabledAt) : DomainEvent;
