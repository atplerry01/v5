using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.CoreObject.Asset;

public sealed record AssetCreatedEvent(
    [property: JsonPropertyName("AggregateId")] AssetId AssetId,
    AssetTitle Title,
    AssetClassification Classification,
    Timestamp CreatedAt) : DomainEvent;
