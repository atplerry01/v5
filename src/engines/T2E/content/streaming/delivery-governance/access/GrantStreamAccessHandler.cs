using Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.Access;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Streaming.DeliveryGovernance.Access;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Streaming.DeliveryGovernance.Access;

public sealed class GrantStreamAccessHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not GrantStreamAccessCommand cmd) return Task.CompletedTask;
        var aggregate = StreamAccessAggregate.Grant(
            new StreamAccessId(cmd.AccessId),
            new StreamRef(cmd.StreamId),
            Enum.Parse<AccessMode>(cmd.Mode),
            new AccessWindow(new Timestamp(cmd.WindowStart), new Timestamp(cmd.WindowEnd)),
            new TokenBinding(cmd.Token),
            new Timestamp(cmd.GrantedAt));
        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
