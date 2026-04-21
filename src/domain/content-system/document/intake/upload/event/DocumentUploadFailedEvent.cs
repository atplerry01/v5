using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Intake.Upload;

public sealed record DocumentUploadFailedEvent(
    [property: JsonPropertyName("AggregateId")] DocumentUploadId UploadId,
    DocumentUploadFailureReason Reason,
    Timestamp FailedAt) : DomainEvent;
