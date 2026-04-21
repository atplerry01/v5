using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.CoreObject.Asset;

public sealed record AssetActivatedEvent(
    [property: JsonPropertyName("AggregateId")] AssetId AssetId,
    Timestamp ActivatedAt) : DomainEvent;
