using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.CoreObject.Asset;

public sealed record AssetCreatedEvent(
    AssetId AssetId,
    AssetTitle Title,
    AssetClassification Classification,
    Timestamp CreatedAt) : DomainEvent;
