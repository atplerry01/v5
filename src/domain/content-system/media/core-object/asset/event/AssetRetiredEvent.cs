using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.CoreObject.Asset;

public sealed record AssetRetiredEvent(
    AssetId AssetId,
    Timestamp RetiredAt) : DomainEvent;
