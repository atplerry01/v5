using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.CoreObject.File;

public sealed record DocumentFileArchivedEvent(
    [property: JsonPropertyName("AggregateId")] DocumentFileId DocumentFileId,
    Timestamp ArchivedAt) : DomainEvent;
