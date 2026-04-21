using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Intake.Ingest;

public sealed record MediaIngestCancelledEvent(
    [property: JsonPropertyName("AggregateId")] MediaIngestId UploadId,
    Timestamp CancelledAt) : DomainEvent;
