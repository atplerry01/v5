using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Command.CommandMetadata;

public readonly record struct MetadataSpanId
{
    public string Value { get; }

    public MetadataSpanId(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "MetadataSpanId cannot be empty.");
        Value = value;
    }
}
