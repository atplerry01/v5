using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Intake.Ingest;

public sealed record MediaIngestCompletedEvent(
    [property: JsonPropertyName("AggregateId")] MediaIngestId UploadId,
    MediaIngestOutputRef OutputRef,
    Timestamp CompletedAt) : DomainEvent;
