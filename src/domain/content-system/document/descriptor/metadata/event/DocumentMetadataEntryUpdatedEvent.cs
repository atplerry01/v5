using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Descriptor.Metadata;

public sealed record DocumentMetadataEntryUpdatedEvent(
    [property: JsonPropertyName("AggregateId")] DocumentMetadataId MetadataId,
    MetadataKey Key,
    MetadataValue PreviousValue,
    MetadataValue NewValue,
    Timestamp UpdatedAt) : DomainEvent;
