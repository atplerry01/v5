using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.ContentArtifact.MediaFile;

public static class MediaFileErrors
{
    public static DomainException ChecksumMismatch(string expected, string actual)
        => new($"Checksum mismatch: expected '{expected}', actual '{actual}'.");

    public static DomainException FileAlreadyVerified()
        => new("Media file integrity has already been verified.");

    public static DomainException FileAlreadyCorrupt()
        => new("Media file is already marked corrupt.");

    public static DomainException FileAlreadySuperseded()
        => new("Media file is already superseded.");

    public static DomainException CannotSupersedeWithSelf()
        => new("Successor file id must differ from current file id.");

    public static DomainException InvalidFailureReason()
        => new("Corruption reason cannot be empty.");
}
