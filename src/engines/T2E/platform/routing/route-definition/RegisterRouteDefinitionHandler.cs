using Whycespace.Domain.PlatformSystem.Routing.RouteDefinition;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Platform.Routing.RouteDefinition;

namespace Whycespace.Engines.T2E.Platform.Routing.RouteDefinition;

public sealed class RegisterRouteDefinitionHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RegisterRouteDefinitionCommand cmd)
            return Task.CompletedTask;

        var transport = cmd.TransportHint switch
        {
            "InProcess" => TransportHint.InProcess,
            "Http" => TransportHint.Http,
            "Grpc" => TransportHint.Grpc,
            _ => TransportHint.Kafka
        };

        var aggregate = RouteDefinitionAggregate.Register(
            new RouteDefinitionId(cmd.RouteDefinitionId),
            cmd.RouteName,
            new DomainRoute(cmd.SourceClassification, cmd.SourceContext, cmd.SourceDomain),
            new DomainRoute(cmd.DestinationClassification, cmd.DestinationContext, cmd.DestinationDomain),
            transport,
            cmd.Priority,
            new Timestamp(cmd.RegisteredAt));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
