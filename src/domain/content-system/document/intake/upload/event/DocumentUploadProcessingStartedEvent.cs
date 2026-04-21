using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Intake.Upload;

public sealed record DocumentUploadProcessingStartedEvent(
    [property: JsonPropertyName("AggregateId")] DocumentUploadId UploadId,
    Timestamp StartedAt) : DomainEvent;
