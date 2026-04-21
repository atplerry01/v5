using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.CoreObject.File;

public sealed record DocumentFileRegisteredEvent(
    [property: JsonPropertyName("AggregateId")] DocumentFileId DocumentFileId,
    DocumentRef DocumentRef,
    DocumentFileStorageRef StorageRef,
    DocumentFileChecksum DeclaredChecksum,
    DocumentFileMimeType MimeType,
    DocumentFileSize Size,
    Timestamp RegisteredAt) : DomainEvent;
