using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.CoreObject.Asset;

public sealed record AssetActivatedEvent(
    AssetId AssetId,
    Timestamp ActivatedAt) : DomainEvent;
