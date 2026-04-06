namespace Whyce.Shared.Contracts.Runtime;

public interface IRuntimeControlPlane
{
    Task<CommandResult> ExecuteAsync(object command, CommandContext context);
}
