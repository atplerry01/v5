using Whyce.Shared.Contracts.Runtime;

namespace Whyce.Runtime.Pipeline;

public interface IMiddleware
{
    Task<CommandResult> ExecuteAsync(CommandContext context, object command, Func<Task<CommandResult>> next);
}
