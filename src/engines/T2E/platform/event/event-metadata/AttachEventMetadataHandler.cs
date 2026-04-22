using Whycespace.Domain.PlatformSystem.Event.EventMetadata;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Platform.Event.EventMetadata;

namespace Whycespace.Engines.T2E.Platform.Event.EventMetadata;

public sealed class AttachEventMetadataHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not AttachEventMetadataCommand cmd)
            return Task.CompletedTask;

        var aggregate = EventMetadataAggregate.Attach(
            new EventMetadataId(cmd.EventMetadataId),
            new EventEnvelopeRef(cmd.EnvelopeRef),
            new ExecutionHash(cmd.ExecutionHash),
            new PolicyDecisionHash(cmd.PolicyDecisionHash),
            new EventActorId(cmd.ActorId),
            new EventTraceId(cmd.TraceId),
            new EventSpanId(cmd.SpanId),
            new Timestamp(cmd.IssuedAt));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
