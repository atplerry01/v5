using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Command.CommandDefinition;

public readonly record struct CommandVersion
{
    public int Value { get; }

    public CommandVersion(int value)
    {
        Guard.Against(value < 1, "CommandVersion must be at least 1.");
        Value = value;
    }
}
