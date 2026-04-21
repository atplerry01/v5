using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Descriptor.Metadata;

public sealed record DocumentMetadataCreatedEvent(
    [property: JsonPropertyName("AggregateId")] DocumentMetadataId MetadataId,
    DocumentRef DocumentRef,
    Timestamp CreatedAt) : DomainEvent;
