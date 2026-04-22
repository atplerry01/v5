using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Command.CommandMetadata;

public readonly record struct CommandMetadataId
{
    public Guid Value { get; }

    public CommandMetadataId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "CommandMetadataId cannot be empty.");
        Value = value;
    }
}
