using Whycespace.Runtime.Command;

namespace Whycespace.Runtime.ControlPlane.Middleware;

public delegate Task<CommandResult> MiddlewareDelegate(CommandContext context);

public interface IMiddleware
{
    Task<CommandResult> InvokeAsync(CommandContext context, MiddlewareDelegate next);
}
