using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.StreamCore.Availability;

public sealed record PlaybackArchivedEvent(
    [property: JsonPropertyName("AggregateId")] PlaybackId PlaybackId,
    Timestamp ArchivedAt) : DomainEvent;
