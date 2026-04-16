namespace Whycespace.Domain.ContentSystem.Media.Asset;

public static class MediaAssetIntegrityService
{
    public static bool DigestMatches(MediaAssetAggregate asset, ContentDigest candidate) =>
        asset is not null && candidate is not null && asset.Digest.Value == candidate.Value;

    public static bool StorageMatches(MediaAssetAggregate asset, StorageLocation candidate) =>
        asset is not null
        && candidate is not null
        && asset.Storage.Uri == candidate.Uri
        && asset.Storage.SizeBytes == candidate.SizeBytes;

    public static bool RepresentsSameBytes(MediaAssetAggregate left, MediaAssetAggregate right) =>
        left is not null && right is not null && left.Digest.Value == right.Digest.Value;
}
