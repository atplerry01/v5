using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Command.CommandEnvelope;

public readonly record struct CommandId
{
    public Guid Value { get; }

    public CommandId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "CommandId cannot be empty.");
        Value = value;
    }
}
