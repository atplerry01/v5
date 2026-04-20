using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.ContentArtifact.Asset;

public sealed record AssetRenamedEvent(
    AssetId AssetId,
    AssetTitle PreviousTitle,
    AssetTitle NewTitle,
    Timestamp RenamedAt) : DomainEvent;
