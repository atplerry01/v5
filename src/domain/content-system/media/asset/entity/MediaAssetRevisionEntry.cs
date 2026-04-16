using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Asset;

public sealed class MediaAssetRevisionEntry
{
    public int Revision { get; }
    public ContentDigest Digest { get; }
    public StorageLocation Storage { get; }
    public Timestamp RecordedAt { get; }

    private MediaAssetRevisionEntry(int revision, ContentDigest digest, StorageLocation storage, Timestamp recordedAt)
    {
        Revision = revision;
        Digest = digest;
        Storage = storage;
        RecordedAt = recordedAt;
    }

    public static MediaAssetRevisionEntry Create(int revision, ContentDigest digest, StorageLocation storage, Timestamp recordedAt)
    {
        if (revision <= 0)
            throw new DomainException("Revision number must be a positive integer.");
        return new MediaAssetRevisionEntry(revision, digest, storage, recordedAt);
    }
}
