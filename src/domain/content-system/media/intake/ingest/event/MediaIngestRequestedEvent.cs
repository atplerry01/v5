using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Intake.Ingest;

public sealed record MediaIngestRequestedEvent(
    [property: JsonPropertyName("AggregateId")] MediaIngestId UploadId,
    MediaIngestSourceRef SourceRef,
    MediaIngestInputRef InputRef,
    Timestamp RequestedAt) : DomainEvent;
