using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Command.CommandRouting;

public readonly record struct CommandRoutingRuleId
{
    public Guid Value { get; }

    public CommandRoutingRuleId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "CommandRoutingRuleId cannot be empty.");
        Value = value;
    }
}
