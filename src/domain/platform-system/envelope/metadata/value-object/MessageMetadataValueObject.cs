using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Envelope.Metadata;

public sealed record MessageMetadataValueObject
{
    public Guid CorrelationId { get; }
    public Guid CausationId { get; }
    public int MessageVersion { get; }
    public Timestamp IssuedAt { get; }
    public Guid? TenantId { get; }

    public MessageMetadataValueObject(
        Guid correlationId,
        Guid causationId,
        int messageVersion,
        Timestamp issuedAt,
        Guid? tenantId)
    {
        Guard.Against(correlationId == Guid.Empty, "MessageMetadata requires a non-empty CorrelationId.");
        Guard.Against(causationId == Guid.Empty, "MessageMetadata requires a non-empty CausationId.");
        Guard.Against(messageVersion < 1, "MessageMetadata MessageVersion must be ≥ 1.");

        CorrelationId = correlationId;
        CausationId = causationId;
        MessageVersion = messageVersion;
        IssuedAt = issuedAt;
        TenantId = tenantId;
    }
}
