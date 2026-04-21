using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Intake.Ingest;

public sealed record MediaIngestProcessingStartedEvent(
    [property: JsonPropertyName("AggregateId")] MediaIngestId UploadId,
    Timestamp StartedAt) : DomainEvent;
