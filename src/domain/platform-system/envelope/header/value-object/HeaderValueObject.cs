using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Envelope.Header;

public sealed record HeaderValueObject
{
    public Guid MessageId { get; }
    public string ContentType { get; }
    public string MessageKindHint { get; }
    public DomainRoute SourceAddress { get; }
    public DomainRoute? DestinationAddress { get; }
    public string TraceId { get; }
    public string SpanId { get; }
    public string? ParentSpanId { get; }
    public bool SamplingFlag { get; }

    public HeaderValueObject(
        Guid messageId,
        string contentType,
        string messageKindHint,
        DomainRoute sourceAddress,
        DomainRoute? destinationAddress,
        string traceId,
        string spanId,
        string? parentSpanId,
        bool samplingFlag)
    {
        Guard.Against(messageId == Guid.Empty, "HeaderValueObject requires a non-empty MessageId.");
        Guard.Against(string.IsNullOrWhiteSpace(contentType), "HeaderValueObject requires a non-empty ContentType.");
        Guard.Against(string.IsNullOrWhiteSpace(messageKindHint), "HeaderValueObject requires a non-empty MessageKindHint.");
        Guard.Against(!sourceAddress.IsValid(), "HeaderValueObject requires a valid SourceAddress.");
        Guard.Against(string.IsNullOrWhiteSpace(traceId), "HeaderValueObject requires a non-empty TraceId.");
        Guard.Against(string.IsNullOrWhiteSpace(spanId), "HeaderValueObject requires a non-empty SpanId.");

        MessageId = messageId;
        ContentType = contentType;
        MessageKindHint = messageKindHint;
        SourceAddress = sourceAddress;
        DestinationAddress = destinationAddress;
        TraceId = traceId;
        SpanId = spanId;
        ParentSpanId = parentSpanId;
        SamplingFlag = samplingFlag;
    }
}
