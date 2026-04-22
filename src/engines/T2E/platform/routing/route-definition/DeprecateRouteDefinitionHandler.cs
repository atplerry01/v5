using Whycespace.Domain.PlatformSystem.Routing.RouteDefinition;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Platform.Routing.RouteDefinition;

namespace Whycespace.Engines.T2E.Platform.Routing.RouteDefinition;

public sealed class DeprecateRouteDefinitionHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not DeprecateRouteDefinitionCommand cmd)
            return;

        var aggregate = (RouteDefinitionAggregate)await context.LoadAggregateAsync(typeof(RouteDefinitionAggregate));
        aggregate.Deprecate(new Timestamp(cmd.DeprecatedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
