using Whycespace.Domain.PlatformSystem.Event.EventStream;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Platform.Event.EventStream;

namespace Whycespace.Engines.T2E.Platform.Event.EventStream;

public sealed class DeclareEventStreamHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not DeclareEventStreamCommand cmd)
            return Task.CompletedTask;

        var ordering = cmd.OrderingGuarantee switch
        {
            "Unordered" => OrderingGuarantee.Unordered,
            _ => OrderingGuarantee.Ordered
        };

        var aggregate = EventStreamAggregate.Declare(
            new EventStreamId(cmd.EventStreamId),
            new DomainRoute(cmd.SourceClassification, cmd.SourceContext, cmd.SourceDomain),
            cmd.IncludedEventTypes,
            ordering,
            new Timestamp(cmd.DeclaredAt));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
