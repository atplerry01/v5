using Whycespace.Domain.PlatformSystem.Routing.RouteResolution;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Platform.Routing.RouteResolution;

namespace Whycespace.Engines.T2E.Platform.Routing.RouteResolution;

public sealed class FailRouteResolutionHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not FailRouteResolutionCommand cmd)
            return Task.CompletedTask;

        var aggregate = RouteResolutionAggregate.Fail(
            new ResolutionId(cmd.ResolutionId),
            new DomainRoute(cmd.SourceClassification, cmd.SourceContext, cmd.SourceDomain),
            cmd.MessageType,
            cmd.DispatchRulesEvaluated,
            cmd.FailureReason,
            new Timestamp(cmd.ResolvedAt));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
