using Whycespace.Domain.PlatformSystem.Routing.RouteDescriptor;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Platform.Routing.RouteDescriptor;

namespace Whycespace.Engines.T2E.Platform.Routing.RouteDescriptor;

public sealed class RegisterRouteDescriptorHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not RegisterRouteDescriptorCommand cmd)
            return Task.CompletedTask;

        var aggregate = RouteDescriptorAggregate.Register(
            new RouteDescriptorId(cmd.RouteDescriptorId),
            new DomainRoute(cmd.SourceClassification, cmd.SourceContext, cmd.SourceDomain),
            new DomainRoute(cmd.DestinationClassification, cmd.DestinationContext, cmd.DestinationDomain),
            cmd.TransportHint,
            cmd.Priority,
            new Timestamp(cmd.RegisteredAt));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
