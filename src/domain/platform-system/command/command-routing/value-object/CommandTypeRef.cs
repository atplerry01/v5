using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Command.CommandRouting;

public readonly record struct CommandTypeRef
{
    public Guid CommandDefinitionId { get; }

    public CommandTypeRef(Guid commandDefinitionId)
    {
        Guard.Against(commandDefinitionId == Guid.Empty, "CommandTypeRef requires a non-empty CommandDefinitionId.");
        CommandDefinitionId = commandDefinitionId;
    }
}
