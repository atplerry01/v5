using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Descriptor.Metadata;

public sealed record DocumentMetadataFinalizedEvent(
    [property: JsonPropertyName("AggregateId")] DocumentMetadataId MetadataId,
    Timestamp FinalizedAt) : DomainEvent;
