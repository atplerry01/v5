using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.CoreObject.Template;

public sealed record DocumentTemplateDeprecatedEvent(
    [property: JsonPropertyName("AggregateId")] DocumentTemplateId TemplateId,
    string Reason,
    Timestamp DeprecatedAt) : DomainEvent;
