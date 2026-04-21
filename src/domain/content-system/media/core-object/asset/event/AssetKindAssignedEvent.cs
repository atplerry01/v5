using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.CoreObject.Asset;

public sealed record AssetKindAssignedEvent(
    [property: JsonPropertyName("AggregateId")] AssetId AssetId,
    AssetKind PreviousKind,
    AssetKind NewKind,
    Timestamp AssignedAt) : DomainEvent;
