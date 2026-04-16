using Whycespace.Domain.ContentSystem.Interaction.Messaging;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Content.Interaction.Messaging;
using Whycespace.Shared.Contracts.Engine;

namespace Whycespace.Engines.T2E.Content.Interaction.Messaging;

public sealed class SendMessageHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not SendMessageCommand cmd)
            return Task.CompletedTask;

        var aggregate = MessageAggregate.Send(
            new EventId(cmd.CommandId),
            new AggregateId(cmd.AggregateId),
            new CorrelationId(cmd.CorrelationId),
            new CausationId(cmd.CausationId),
            MessageId.From(cmd.MessageId),
            cmd.ConversationRef,
            cmd.SenderRef,
            MessageBody.Create(cmd.Body),
            new Timestamp(cmd.OccurredAt));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
