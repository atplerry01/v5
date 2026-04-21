using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Intake.Ingest;

public sealed record MediaIngestFailedEvent(
    [property: JsonPropertyName("AggregateId")] MediaIngestId UploadId,
    MediaIngestFailureReason Reason,
    Timestamp FailedAt) : DomainEvent;
