using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.CoreObject.Asset;

public sealed record AssetReclassifiedEvent(
    AssetId AssetId,
    AssetClassification PreviousClassification,
    AssetClassification NewClassification,
    Timestamp ReclassifiedAt) : DomainEvent;
