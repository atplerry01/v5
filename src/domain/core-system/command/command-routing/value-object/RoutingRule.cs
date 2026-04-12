namespace Whycespace.Domain.CoreSystem.Command.CommandRouting;

public readonly record struct RoutingRule
{
    public string CommandName { get; }
    public string TargetHandler { get; }

    public RoutingRule(string commandName, string targetHandler)
    {
        if (string.IsNullOrWhiteSpace(commandName))
            throw new ArgumentException("Command name must not be empty.", nameof(commandName));

        if (string.IsNullOrWhiteSpace(targetHandler))
            throw new ArgumentException("Target handler must not be empty.", nameof(targetHandler));

        CommandName = commandName;
        TargetHandler = targetHandler;
    }
}
