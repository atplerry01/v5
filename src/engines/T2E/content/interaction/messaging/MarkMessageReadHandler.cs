using Whycespace.Domain.ContentSystem.Interaction.Messaging;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Interaction.Messaging;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Interaction.Messaging;

public sealed class MarkMessageReadHandler : IEngine
{
    public async Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not MarkMessageReadCommand cmd)
            return;

        var aggregate = (MessageAggregate)await context.LoadAggregateAsync(typeof(MessageAggregate));
        aggregate.MarkRead(
            new EventId(cmd.CommandId),
            new AggregateId(cmd.AggregateId),
            new CorrelationId(cmd.CorrelationId),
            new CausationId(cmd.CausationId),
            cmd.RecipientRef,
            new Timestamp(cmd.OccurredAt));

        context.EmitEvents(aggregate.DomainEvents);
    }
}
