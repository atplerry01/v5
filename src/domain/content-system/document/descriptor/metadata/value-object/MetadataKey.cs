using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Descriptor.Metadata;

public readonly record struct MetadataKey
{
    public string Value { get; }

    public MetadataKey(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "MetadataKey cannot be empty.");
        Guard.Against(value.Length > 128, "MetadataKey cannot exceed 128 characters.");
        var trimmed = value.Trim();
        Guard.Against(
            trimmed.Any(c => !(char.IsLetterOrDigit(c) || c == '.' || c == '-' || c == '_' || c == ':')),
            "MetadataKey may contain only letters, digits, dot, dash, underscore, or colon.");
        Value = trimmed.ToLowerInvariant();
    }

    public override string ToString() => Value;
}
