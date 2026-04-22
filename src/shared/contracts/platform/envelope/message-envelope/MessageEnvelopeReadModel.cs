namespace Whycespace.Shared.Contracts.Platform.Envelope.MessageEnvelope;

public sealed record MessageEnvelopeReadModel
{
    public Guid EnvelopeId { get; init; }
    public string MessageKind { get; init; } = string.Empty;
    public Guid CorrelationId { get; init; }
    public Guid CausationId { get; init; }
    public string SourceClassification { get; init; } = string.Empty;
    public string SourceContext { get; init; } = string.Empty;
    public string SourceDomain { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string? RejectionReason { get; init; }
    public DateTimeOffset IssuedAt { get; init; }
    public DateTimeOffset LastModifiedAt { get; init; }
}
