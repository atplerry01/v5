using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Command.CommandMetadata;

public sealed class CommandMetadataAggregate : AggregateRoot
{
    public CommandMetadataId CommandMetadataId { get; private set; }
    public Guid EnvelopeRef { get; private set; }
    public MetadataActorId ActorId { get; private set; }
    public MetadataTraceId TraceId { get; private set; }
    public MetadataSpanId SpanId { get; private set; }
    public PolicyContextRef PolicyContextRef { get; private set; }
    public TrustScore TrustScore { get; private set; }
    public Timestamp IssuedAt { get; private set; }

    private CommandMetadataAggregate() { }

    public static CommandMetadataAggregate Attach(
        CommandMetadataId id,
        Guid envelopeRef,
        MetadataActorId actorId,
        MetadataTraceId traceId,
        MetadataSpanId spanId,
        PolicyContextRef policyContextRef,
        TrustScore trustScore,
        Timestamp issuedAt)
    {
        var aggregate = new CommandMetadataAggregate();
        if (aggregate.Version >= 0)
            throw CommandMetadataErrors.AlreadyInitialized();

        if (envelopeRef == Guid.Empty)
            throw CommandMetadataErrors.EnvelopeRefMissing();

        aggregate.RaiseDomainEvent(new CommandMetadataAttachedEvent(
            id, envelopeRef, actorId, traceId, spanId, policyContextRef, trustScore, issuedAt));

        return aggregate;
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case CommandMetadataAttachedEvent e:
                CommandMetadataId = e.CommandMetadataId;
                EnvelopeRef = e.EnvelopeRef;
                ActorId = e.ActorId;
                TraceId = e.TraceId;
                SpanId = e.SpanId;
                PolicyContextRef = e.PolicyContextRef;
                TrustScore = e.TrustScore;
                IssuedAt = e.IssuedAt;
                break;
        }
    }
}
