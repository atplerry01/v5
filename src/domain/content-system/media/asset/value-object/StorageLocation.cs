using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Asset;

public sealed record StorageLocation : ValueObject
{
    public string Uri { get; }
    public long SizeBytes { get; }

    private StorageLocation(string uri, long sizeBytes)
    {
        Uri = uri;
        SizeBytes = sizeBytes;
    }

    public static StorageLocation Create(string uri, long sizeBytes)
    {
        if (string.IsNullOrWhiteSpace(uri))
            throw MediaAssetErrors.InvalidStorageUri();
        var normalised = uri.Trim();
        if (!System.Uri.TryCreate(normalised, UriKind.Absolute, out _))
            throw MediaAssetErrors.InvalidStorageUri();
        if (sizeBytes <= 0)
            throw MediaAssetErrors.InvalidStorageSize();
        return new StorageLocation(normalised, sizeBytes);
    }

    public override string ToString() => $"{Uri} ({SizeBytes} bytes)";
}
