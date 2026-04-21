using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Descriptor.Metadata;

public sealed record DocumentMetadataEntryRemovedEvent(
    [property: JsonPropertyName("AggregateId")] DocumentMetadataId MetadataId,
    MetadataKey Key,
    Timestamp RemovedAt) : DomainEvent;
