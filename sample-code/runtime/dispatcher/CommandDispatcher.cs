using Whycespace.Runtime.Command;
using Whycespace.Runtime.Routing;
using Whycespace.Runtime.Workflow;

namespace Whycespace.Runtime.Dispatcher;

public sealed class CommandDispatcher
{
    public const string DomainRouteKey = "Routing.DomainRoute";

    private readonly WorkflowOrchestrator _orchestrator;
    private readonly DomainRouteResolver _routeResolver;

    public CommandDispatcher(WorkflowOrchestrator orchestrator, DomainRouteResolver routeResolver)
    {
        _orchestrator = orchestrator;
        _routeResolver = routeResolver;
    }

    public Task<CommandResult> DispatchAsync(CommandContext context)
    {
        var route = _routeResolver.Resolve(context.Envelope.CommandType);
        if (route is not null)
        {
            context.Set(DomainRouteKey, route);
        }

        return _orchestrator.OrchestrateAsync(context);
    }
}
