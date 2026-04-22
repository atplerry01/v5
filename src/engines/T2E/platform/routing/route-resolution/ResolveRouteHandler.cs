using Whycespace.Domain.PlatformSystem.Routing.RouteResolution;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Platform.Routing.RouteResolution;

namespace Whycespace.Engines.T2E.Platform.Routing.RouteResolution;

public sealed class ResolveRouteHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not ResolveRouteCommand cmd)
            return Task.CompletedTask;

        var strategy = cmd.ResolutionStrategy switch
        {
            "PrefixMatch" => ResolutionStrategy.PrefixMatch,
            "DefaultRoute" => ResolutionStrategy.DefaultRoute,
            _ => ResolutionStrategy.ExactMatch
        };

        var aggregate = RouteResolutionAggregate.Resolve(
            new ResolutionId(cmd.ResolutionId),
            new DomainRoute(cmd.SourceClassification, cmd.SourceContext, cmd.SourceDomain),
            cmd.MessageType,
            cmd.ResolvedRouteRef,
            strategy,
            cmd.DispatchRulesEvaluated,
            new Timestamp(cmd.ResolvedAt));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
