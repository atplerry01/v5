using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Document.Descriptor.Metadata;

public readonly record struct MetadataValue
{
    public string Value { get; }

    public MetadataValue(string value)
    {
        Guard.Against(value is null, "MetadataValue cannot be null.");
        Guard.Against(value!.Length > 4096, "MetadataValue cannot exceed 4096 characters.");
        Value = value;
    }

    public override string ToString() => Value;
}
