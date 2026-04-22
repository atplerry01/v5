using Whycespace.Domain.PlatformSystem.Envelope.MessageEnvelope;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Platform.Envelope.MessageEnvelope;

namespace Whycespace.Engines.T2E.Platform.Envelope.MessageEnvelope;

public sealed class AcknowledgeMessageEnvelopeHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not AcknowledgeMessageEnvelopeCommand cmd)
            return;

        var aggregate = (MessageEnvelopeAggregate)await context.LoadAggregateAsync(typeof(MessageEnvelopeAggregate));
        aggregate.Acknowledge(new Timestamp(cmd.AcknowledgedAt));
        context.EmitEvents(aggregate.DomainEvents);
    }
}
