using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.CoreObject.Asset;

public sealed record AssetReclassifiedEvent(
    [property: JsonPropertyName("AggregateId")] AssetId AssetId,
    AssetClassification PreviousClassification,
    AssetClassification NewClassification,
    Timestamp ReclassifiedAt) : DomainEvent;
