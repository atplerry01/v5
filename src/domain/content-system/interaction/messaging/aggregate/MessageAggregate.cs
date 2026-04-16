using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Interaction.Messaging;

public sealed class MessageAggregate : AggregateRoot
{
    private static readonly MessagingSpecification Spec = new();
    private readonly List<MessageAttachment> _attachments = new();

    public MessageId MessageId { get; private set; }
    public string ConversationRef { get; private set; } = string.Empty;
    public string SenderRef { get; private set; } = string.Empty;
    public MessageBody Body { get; private set; } = default!;
    public MessageStatus Status { get; private set; }
    public Timestamp SentAt { get; private set; }
    public IReadOnlyList<MessageAttachment> Attachments => _attachments.AsReadOnly();

    private MessageAggregate() { }

    public static MessageAggregate Send(
        EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId,
        MessageId messageId, string conversationRef, string senderRef, MessageBody body, Timestamp sentAt)
    {
        Spec.EnsureSatisfied(new MessageDispatchCandidate(messageId, conversationRef, senderRef, body));
        var aggregate = new MessageAggregate();
        aggregate.RaiseDomainEvent(new MessageSentEvent(
            eventId, aggregateId, correlationId, causationId,
            messageId, conversationRef, senderRef, body.Value, sentAt));
        return aggregate;
    }

    public void MarkDelivered(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, string recipientRef, Timestamp at)
    {
        if (string.IsNullOrWhiteSpace(recipientRef)) throw MessagingErrors.InvalidSender();
        Spec.EnsureTransition(Status, MessageStatus.Delivered);
        RaiseDomainEvent(new MessageDeliveredEvent(eventId, aggregateId, correlationId, causationId, MessageId, recipientRef, at));
    }

    public void MarkRead(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, string recipientRef, Timestamp at)
    {
        if (string.IsNullOrWhiteSpace(recipientRef)) throw MessagingErrors.InvalidSender();
        Spec.EnsureTransition(Status, MessageStatus.Read);
        RaiseDomainEvent(new MessageReadEvent(eventId, aggregateId, correlationId, causationId, MessageId, recipientRef, at));
    }

    public void Edit(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, MessageBody newBody, Timestamp at)
    {
        if (Status == MessageStatus.Read) throw MessagingErrors.CannotEditAfterRead();
        if (Status == MessageStatus.Retracted) throw MessagingErrors.AlreadyRetracted();
        if (Status == MessageStatus.Draft) throw MessagingErrors.NotSent();
        RaiseDomainEvent(new MessageEditedEvent(eventId, aggregateId, correlationId, causationId, MessageId, newBody.Value, at));
    }

    public void Retract(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, Timestamp at)
    {
        if (Status == MessageStatus.Retracted) throw MessagingErrors.AlreadyRetracted();
        Spec.EnsureTransition(Status, MessageStatus.Retracted);
        RaiseDomainEvent(new MessageRetractedEvent(eventId, aggregateId, correlationId, causationId, MessageId, at));
    }

    public void AttachFile(MessageAttachment attachment)
    {
        if (attachment is null) throw MessagingErrors.InvalidAttachment();
        if (_attachments.Any(a => a.AttachmentId == attachment.AttachmentId))
            throw MessagingErrors.InvalidAttachment();
        _attachments.Add(attachment);
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case MessageSentEvent e:
                MessageId = e.MessageId;
                ConversationRef = e.ConversationRef;
                SenderRef = e.SenderRef;
                Body = MessageBody.Create(e.Body);
                Status = MessageStatus.Sent;
                SentAt = e.SentAt;
                break;
            case MessageDeliveredEvent: Status = MessageStatus.Delivered; break;
            case MessageReadEvent: Status = MessageStatus.Read; break;
            case MessageEditedEvent e: Body = MessageBody.Create(e.Body); break;
            case MessageRetractedEvent: Status = MessageStatus.Retracted; break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Body is null) return;
        if (string.IsNullOrWhiteSpace(SenderRef)) throw MessagingErrors.SenderMissing();
        if (string.IsNullOrWhiteSpace(ConversationRef)) throw MessagingErrors.ConversationMissing();
    }
}
