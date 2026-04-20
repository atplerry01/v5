using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Media.Descriptor.Metadata;

public readonly record struct CodecName
{
    public string Value { get; }

    public CodecName(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "CodecName cannot be empty.");
        Guard.Against(value.Length > 64, "CodecName cannot exceed 64 characters.");
        Value = value.Trim().ToLowerInvariant();
    }

    public override string ToString() => Value;
}
