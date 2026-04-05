namespace Whyce.Shared.Contracts.Runtime;

public interface ICommandDispatcher
{
    Task<CommandResult> DispatchAsync(object command, CommandContext context);
}
