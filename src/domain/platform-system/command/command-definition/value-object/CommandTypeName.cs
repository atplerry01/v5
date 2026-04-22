using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Command.CommandDefinition;

public readonly record struct CommandTypeName
{
    public string Value { get; }

    public CommandTypeName(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "CommandTypeName cannot be empty.");
        Value = value;
    }
}
