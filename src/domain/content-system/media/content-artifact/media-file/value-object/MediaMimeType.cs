using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.ContentArtifact.MediaFile;

public readonly record struct MediaMimeType
{
    public string Value { get; }

    public MediaMimeType(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "MediaMimeType cannot be empty.");
        Guard.Against(!value.Contains('/'), "MediaMimeType must contain '/' (e.g. 'image/png').");
        Guard.Against(value.Length > 128, "MediaMimeType cannot exceed 128 characters.");
        Value = value.Trim().ToLowerInvariant();
    }

    public override string ToString() => Value;
}
