using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Command.CommandRouting;

public readonly record struct RoutingRule
{
    public string CommandName { get; }
    public string TargetHandler { get; }

    public RoutingRule(string commandName, string targetHandler)
    {
        Guard.Against(string.IsNullOrWhiteSpace(commandName), "Command name must not be empty.");
        Guard.Against(string.IsNullOrWhiteSpace(targetHandler), "Target handler must not be empty.");

        CommandName = commandName;
        TargetHandler = targetHandler;
    }
}
