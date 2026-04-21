using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Command.CommandRouting;

public readonly record struct CommandRoutingId
{
    public Guid Value { get; }

    public CommandRoutingId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "CommandRoutingId cannot be empty.");
        Value = value;
    }
}
