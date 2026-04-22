using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Command.CommandDefinition;

public readonly record struct CommandDefinitionId
{
    public Guid Value { get; }

    public CommandDefinitionId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "CommandDefinitionId cannot be empty.");
        Value = value;
    }
}
