using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Intake.Upload;

public sealed record DocumentUploadRequestedEvent(
    [property: JsonPropertyName("AggregateId")] DocumentUploadId UploadId,
    DocumentUploadSourceRef SourceRef,
    DocumentUploadInputRef InputRef,
    Timestamp RequestedAt) : DomainEvent;
