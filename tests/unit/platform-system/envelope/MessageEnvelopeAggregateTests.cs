using Whycespace.Domain.PlatformSystem.Envelope.Header;
using Whycespace.Domain.PlatformSystem.Envelope.MessageEnvelope;
using Whycespace.Domain.PlatformSystem.Envelope.Metadata;
using Whycespace.Domain.PlatformSystem.Envelope.Payload;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Tests.Shared;

namespace Whycespace.Tests.Unit.PlatformSystem.Envelope;

public sealed class MessageEnvelopeAggregateTests
{
    private static readonly TestIdGenerator IdGen = new();
    private static readonly Timestamp Now = new(new DateTimeOffset(2026, 4, 22, 10, 0, 0, TimeSpan.Zero));

    private static readonly DomainRoute SourceAddr = new("economic", "capital", "account");
    private static readonly DomainRoute DestAddr = new("platform", "routing", "dispatch-rule");

    private static MessageEnvelopeAggregate NewCreated(string seed)
    {
        var id = new EnvelopeId(IdGen.Generate($"MessageEnvelopeAggregateTests:{seed}"));
        var messageId = IdGen.Generate($"MessageEnvelopeAggregateTests:{seed}:msg");
        var header = new HeaderValueObject(messageId, "application/json", "Command",
            SourceAddr, DestAddr, "trace-1", "span-1", null, false);
        var payload = new PayloadValueObject("CreateAccount", PayloadEncoding.Json, null, new byte[] { 1, 2, 3 });
        var metadata = new MessageMetadataValueObject(IdGen.Generate($"{seed}:corr"), IdGen.Generate($"{seed}:cause"), 1, Now, null);
        return MessageEnvelopeAggregate.Create(id, header, payload, metadata, MessageKind.Command, Now);
    }

    [Fact]
    public void Create_WithValidArgs_RaisesMessageEnvelopeCreatedEvent()
    {
        var aggregate = NewCreated("Create");

        Assert.Equal(EnvelopeStatus.Created, aggregate.Status);
        Assert.Equal(MessageKind.Command, aggregate.MessageKind);

        var evt = Assert.IsType<MessageEnvelopeCreatedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal("Command", evt.MessageKind.Value);
    }

    [Fact]
    public void Dispatch_FromCreated_TransitionsToDispatched()
    {
        var aggregate = NewCreated("Dispatch");
        aggregate.ClearDomainEvents();

        aggregate.Dispatch(Now);

        Assert.Equal(EnvelopeStatus.Dispatched, aggregate.Status);
        Assert.IsType<MessageEnvelopeDispatchedEvent>(Assert.Single(aggregate.DomainEvents));
    }

    [Fact]
    public void Acknowledge_FromDispatched_TransitionsToAcknowledged()
    {
        var aggregate = NewCreated("Acknowledge");
        aggregate.Dispatch(Now);
        aggregate.ClearDomainEvents();

        aggregate.Acknowledge(Now);

        Assert.Equal(EnvelopeStatus.Acknowledged, aggregate.Status);
        Assert.IsType<MessageEnvelopeAcknowledgedEvent>(Assert.Single(aggregate.DomainEvents));
    }

    [Fact]
    public void Reject_FromDispatched_TransitionsToRejected()
    {
        var aggregate = NewCreated("Reject");
        aggregate.Dispatch(Now);
        aggregate.ClearDomainEvents();

        aggregate.Reject("invalid payload", Now);

        Assert.Equal(EnvelopeStatus.Rejected, aggregate.Status);
        var evt = Assert.IsType<MessageEnvelopeRejectedEvent>(Assert.Single(aggregate.DomainEvents));
        Assert.Equal("invalid payload", evt.RejectionReason);
    }

    [Fact]
    public void Dispatch_WhenAlreadyDispatched_Throws()
    {
        var aggregate = NewCreated("DoubleDispatch");
        aggregate.Dispatch(Now);

        Assert.Throws<DomainInvariantViolationException>(() => aggregate.Dispatch(Now));
    }

    [Fact]
    public void Acknowledge_FromCreated_Throws()
    {
        var aggregate = NewCreated("AckFromCreated");

        Assert.Throws<DomainInvariantViolationException>(() => aggregate.Acknowledge(Now));
    }

    [Fact]
    public void Reject_FromCreated_Throws()
    {
        var aggregate = NewCreated("RejectFromCreated");

        Assert.Throws<DomainInvariantViolationException>(() => aggregate.Reject("reason", Now));
    }

    [Fact]
    public void Dispatch_WhenAcknowledged_Throws()
    {
        var aggregate = NewCreated("DispatchAfterAck");
        aggregate.Dispatch(Now);
        aggregate.Acknowledge(Now);

        Assert.Throws<DomainInvariantViolationException>(() => aggregate.Dispatch(Now));
    }

    [Fact]
    public void Dispatch_WhenRejected_Throws()
    {
        var aggregate = NewCreated("DispatchAfterReject");
        aggregate.Dispatch(Now);
        aggregate.Reject("bad", Now);

        Assert.Throws<DomainInvariantViolationException>(() => aggregate.Dispatch(Now));
    }
}
