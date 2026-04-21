using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.CoreObject.File;

public sealed record DocumentFileSupersededEvent(
    [property: JsonPropertyName("AggregateId")] DocumentFileId DocumentFileId,
    DocumentFileId SuccessorFileId,
    Timestamp SupersededAt) : DomainEvent;
