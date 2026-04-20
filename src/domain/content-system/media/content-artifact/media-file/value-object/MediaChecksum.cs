using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.ContentArtifact.MediaFile;

public readonly record struct MediaChecksum
{
    public string Value { get; }

    public MediaChecksum(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "MediaChecksum cannot be empty.");
        Guard.Against(value.Length != 64, "MediaChecksum must be a 64-char SHA256 hex string.");
        Value = value.ToLowerInvariant();
    }

    public override string ToString() => Value;
}
