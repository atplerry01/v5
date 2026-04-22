using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Command.CommandMetadata;

public readonly record struct MetadataTraceId
{
    public string Value { get; }

    public MetadataTraceId(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "MetadataTraceId cannot be empty.");
        Value = value;
    }
}
