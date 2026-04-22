using Whycespace.Domain.PlatformSystem.Envelope.Header;
using Whycespace.Domain.PlatformSystem.Envelope.Metadata;
using Whycespace.Domain.PlatformSystem.Envelope.Payload;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Envelope.MessageEnvelope;

public sealed class MessageEnvelopeAggregate : AggregateRoot
{
    public EnvelopeId EnvelopeId { get; private set; }
    public HeaderValueObject Header { get; private set; } = null!;
    public PayloadValueObject Payload { get; private set; } = null!;
    public MessageMetadataValueObject Metadata { get; private set; } = null!;
    public MessageKind MessageKind { get; private set; }
    public EnvelopeStatus Status { get; private set; }

    private MessageEnvelopeAggregate() { }

    public static MessageEnvelopeAggregate Create(
        EnvelopeId id,
        HeaderValueObject header,
        PayloadValueObject payload,
        MessageMetadataValueObject metadata,
        MessageKind messageKind,
        Timestamp createdAt)
    {
        var aggregate = new MessageEnvelopeAggregate();
        if (aggregate.Version >= 0)
            throw MessageEnvelopeErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new MessageEnvelopeCreatedEvent(
            id, header, payload, metadata, messageKind, createdAt));

        return aggregate;
    }

    public void Dispatch(Timestamp dispatchedAt)
    {
        if (Status.IsTerminal)
            throw MessageEnvelopeErrors.AlreadyTerminated();

        if (Status == EnvelopeStatus.Dispatched)
            throw MessageEnvelopeErrors.AlreadyDispatched();

        RaiseDomainEvent(new MessageEnvelopeDispatchedEvent(EnvelopeId, dispatchedAt));
    }

    public void Acknowledge(Timestamp acknowledgedAt)
    {
        if (Status.IsTerminal)
            throw MessageEnvelopeErrors.AlreadyTerminated();

        if (Status != EnvelopeStatus.Dispatched)
            throw MessageEnvelopeErrors.NotDispatched();

        RaiseDomainEvent(new MessageEnvelopeAcknowledgedEvent(EnvelopeId, acknowledgedAt));
    }

    public void Reject(string rejectionReason, Timestamp rejectedAt)
    {
        if (Status.IsTerminal)
            throw MessageEnvelopeErrors.AlreadyTerminated();

        if (Status != EnvelopeStatus.Dispatched)
            throw MessageEnvelopeErrors.NotDispatched();

        RaiseDomainEvent(new MessageEnvelopeRejectedEvent(EnvelopeId, rejectionReason, rejectedAt));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case MessageEnvelopeCreatedEvent e:
                EnvelopeId = e.EnvelopeId;
                Header = e.Header;
                Payload = e.Payload;
                Metadata = e.Metadata;
                MessageKind = e.MessageKind;
                Status = EnvelopeStatus.Created;
                break;

            case MessageEnvelopeDispatchedEvent:
                Status = EnvelopeStatus.Dispatched;
                break;

            case MessageEnvelopeAcknowledgedEvent:
                Status = EnvelopeStatus.Acknowledged;
                break;

            case MessageEnvelopeRejectedEvent:
                Status = EnvelopeStatus.Rejected;
                break;
        }
    }
}
