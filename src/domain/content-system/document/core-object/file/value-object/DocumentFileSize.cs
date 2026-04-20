using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.CoreObject.File;

public readonly record struct DocumentFileSize
{
    public long Bytes { get; }

    public DocumentFileSize(long bytes)
    {
        Guard.Against(bytes < 0, "DocumentFileSize cannot be negative.");
        Bytes = bytes;
    }

    public override string ToString() => $"{Bytes} bytes";
}
