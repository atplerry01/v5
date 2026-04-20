using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.ContentArtifact.MediaFile;

public readonly record struct FileSize
{
    public long Bytes { get; }

    public FileSize(long bytes)
    {
        Guard.Against(bytes < 0, "FileSize cannot be negative.");
        Bytes = bytes;
    }

    public override string ToString() => $"{Bytes} bytes";
}
