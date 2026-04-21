using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.Availability;

public sealed record PlaybackDisabledEvent(
    [property: JsonPropertyName("AggregateId")] PlaybackId PlaybackId,
    string Reason,
    Timestamp DisabledAt) : DomainEvent;
