using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Command.CommandEnvelope;

public readonly record struct CommandType
{
    public string Value { get; }

    public CommandType(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "CommandType cannot be empty.");
        Value = value;
    }
}
