using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Asset;

public sealed record MediaAssetRegistrationCandidate(
    MediaAssetId AssetId,
    string OwnerRef,
    MediaTitle Title,
    MediaDescription Description,
    MediaType MediaType,
    ContentDigest Digest,
    StorageLocation Storage);

public sealed class MediaAssetRegistrationSpecification : Specification<MediaAssetRegistrationCandidate>
{
    public override bool IsSatisfiedBy(MediaAssetRegistrationCandidate entity)
    {
        if (entity is null) return false;
        if (entity.AssetId.Value == Guid.Empty) return false;
        if (string.IsNullOrWhiteSpace(entity.OwnerRef)) return false;
        if (string.IsNullOrWhiteSpace(entity.Title.Value)) return false;
        if (string.IsNullOrWhiteSpace(entity.Digest.Value)) return false;
        if (string.IsNullOrWhiteSpace(entity.Storage.Uri)) return false;
        if (entity.Storage.SizeBytes <= 0) return false;
        return true;
    }

    public void EnsureSatisfied(MediaAssetRegistrationCandidate candidate)
    {
        if (candidate is null)
            throw MediaAssetErrors.AssetNotRegistered();
        if (candidate.AssetId.Value == Guid.Empty)
            throw MediaAssetErrors.InvalidAssetId();
        if (string.IsNullOrWhiteSpace(candidate.OwnerRef))
            throw MediaAssetErrors.InvalidOwner();
        if (string.IsNullOrWhiteSpace(candidate.Title.Value))
            throw MediaAssetErrors.InvalidTitle();
        if (string.IsNullOrWhiteSpace(candidate.Digest.Value))
            throw MediaAssetErrors.InvalidContentDigest();
        if (string.IsNullOrWhiteSpace(candidate.Storage.Uri))
            throw MediaAssetErrors.InvalidStorageUri();
        if (candidate.Storage.SizeBytes <= 0)
            throw MediaAssetErrors.InvalidStorageSize();
    }
}
