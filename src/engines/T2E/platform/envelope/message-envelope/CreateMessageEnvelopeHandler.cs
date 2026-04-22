using Whycespace.Domain.PlatformSystem.Envelope.Header;
using Whycespace.Domain.PlatformSystem.Envelope.MessageEnvelope;
using Whycespace.Domain.PlatformSystem.Envelope.Metadata;
using Whycespace.Domain.PlatformSystem.Envelope.Payload;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Shared.Contracts.Engine;
using Whycespace.Shared.Contracts.Platform.Envelope.MessageEnvelope;

namespace Whycespace.Engines.T2E.Platform.Envelope.MessageEnvelope;

public sealed class CreateMessageEnvelopeHandler : IEngine
{
    public Task ExecuteAsync(IEngineContext context)
    {
        if (context.Command is not CreateMessageEnvelopeCommand cmd)
            return Task.CompletedTask;

        DomainRoute? destination = cmd.HeaderDestinationClassification is not null
            ? new DomainRoute(cmd.HeaderDestinationClassification, cmd.HeaderDestinationContext!, cmd.HeaderDestinationDomain!)
            : null;

        var header = new HeaderValueObject(
            cmd.HeaderMessageId,
            cmd.HeaderContentType,
            cmd.HeaderMessageKindHint,
            new DomainRoute(cmd.HeaderSourceClassification, cmd.HeaderSourceContext, cmd.HeaderSourceDomain),
            destination,
            cmd.HeaderTraceId,
            cmd.HeaderSpanId,
            cmd.HeaderParentSpanId,
            cmd.HeaderSamplingFlag);

        var encoding = cmd.PayloadEncoding switch
        {
            "Avro" => PayloadEncoding.Avro,
            "Protobuf" => PayloadEncoding.Protobuf,
            "Binary" => PayloadEncoding.Binary,
            _ => PayloadEncoding.Json
        };

        var payload = new PayloadValueObject(
            cmd.PayloadTypeRef,
            encoding,
            cmd.PayloadSchemaRef,
            cmd.PayloadBytes);

        var metadata = new MessageMetadataValueObject(
            cmd.MetadataCorrelationId,
            cmd.MetadataCausationId,
            cmd.MetadataMessageVersion,
            new Timestamp(cmd.MetadataIssuedAt),
            cmd.MetadataTenantId);

        var messageKind = cmd.MessageKind switch
        {
            "Event" => Domain.PlatformSystem.Envelope.MessageEnvelope.MessageKind.Event,
            "Notification" => Domain.PlatformSystem.Envelope.MessageEnvelope.MessageKind.Notification,
            "Query" => Domain.PlatformSystem.Envelope.MessageEnvelope.MessageKind.Query,
            _ => Domain.PlatformSystem.Envelope.MessageEnvelope.MessageKind.Command
        };

        var aggregate = MessageEnvelopeAggregate.Create(
            new EnvelopeId(cmd.EnvelopeId),
            header,
            payload,
            metadata,
            messageKind,
            new Timestamp(cmd.CreatedAt));

        context.EmitEvents(aggregate.DomainEvents);
        return Task.CompletedTask;
    }
}
