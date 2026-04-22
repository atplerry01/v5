using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Command.CommandMetadata;

public readonly record struct MetadataActorId
{
    public string Value { get; }

    public MetadataActorId(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "MetadataActorId cannot be empty.");
        Value = value;
    }
}
