using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Descriptor.Metadata;

public readonly record struct MediaMetadataKey
{
    public string Value { get; }

    public MediaMetadataKey(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "MediaMetadataKey cannot be empty.");
        Guard.Against(value.Length > 128, "MediaMetadataKey cannot exceed 128 characters.");
        var trimmed = value.Trim();
        Guard.Against(
            trimmed.Any(c => !(char.IsLetterOrDigit(c) || c == '.' || c == '-' || c == '_' || c == ':')),
            "MediaMetadataKey may contain only letters, digits, dot, dash, underscore, or colon.");
        Value = trimmed.ToLowerInvariant();
    }

    public override string ToString() => Value;
}
