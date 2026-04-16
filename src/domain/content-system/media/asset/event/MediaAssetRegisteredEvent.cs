using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Asset;

public sealed record MediaAssetRegisteredEvent(
    EventId EventId,
    AggregateId AggregateId,
    CorrelationId CorrelationId,
    CausationId CausationId,
    MediaAssetId MediaAssetId,
    string OwnerRef,
    MediaType MediaType,
    string Title,
    string Description,
    string ContentDigest,
    string StorageUri,
    long StorageSizeBytes,
    Timestamp RegisteredAt) : DomainEvent;
