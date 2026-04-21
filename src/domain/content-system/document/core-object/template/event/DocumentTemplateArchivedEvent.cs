using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.CoreObject.Template;

public sealed record DocumentTemplateArchivedEvent(
    [property: JsonPropertyName("AggregateId")] DocumentTemplateId TemplateId,
    Timestamp ArchivedAt) : DomainEvent;
