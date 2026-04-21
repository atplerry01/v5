using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Intake.Upload;

public sealed record DocumentUploadCompletedEvent(
    [property: JsonPropertyName("AggregateId")] DocumentUploadId UploadId,
    DocumentUploadOutputRef OutputRef,
    Timestamp CompletedAt) : DomainEvent;
