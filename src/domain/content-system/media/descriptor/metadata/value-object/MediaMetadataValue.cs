using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Descriptor.Metadata;

public readonly record struct MediaMetadataValue
{
    public string Value { get; }

    public MediaMetadataValue(string value)
    {
        Guard.Against(value is null, "MediaMetadataValue cannot be null.");
        Guard.Against(value!.Length > 4096, "MediaMetadataValue cannot exceed 4096 characters.");
        Value = value;
    }

    public override string ToString() => Value;
}
