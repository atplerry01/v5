using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.CoreObject.Asset;

public sealed record AssetRenamedEvent(
    [property: JsonPropertyName("AggregateId")] AssetId AssetId,
    AssetTitle PreviousTitle,
    AssetTitle NewTitle,
    Timestamp RenamedAt) : DomainEvent;
