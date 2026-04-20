using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.IntegrationSystem.OutboundEffect;

/// <summary>
/// R3.B.1 / ratified constraint #2 — provider accepted the operation and
/// returned an operation id. Named fields are first-class (not free-text
/// observability). Aggregate <c>Apply</c> asserts non-empty
/// <see cref="ProviderId"/> + <see cref="ProviderOperationId"/> per
/// R-OUT-EFF-FINALITY-02.
/// </summary>
public sealed record OutboundEffectAcknowledgedEvent(
    AggregateId AggregateId,
    string ProviderId,
    string ProviderOperationId,
    string? IdempotencyKeyUsed = null,
    string? AckPayloadDigest = null) : DomainEvent;
