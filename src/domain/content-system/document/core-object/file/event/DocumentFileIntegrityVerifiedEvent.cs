using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.CoreObject.File;

public sealed record DocumentFileIntegrityVerifiedEvent(
    [property: JsonPropertyName("AggregateId")] DocumentFileId DocumentFileId,
    DocumentFileChecksum VerifiedChecksum,
    Timestamp VerifiedAt) : DomainEvent;
