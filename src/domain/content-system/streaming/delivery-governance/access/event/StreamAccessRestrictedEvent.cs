using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.Access;

public sealed record StreamAccessRestrictedEvent(
    StreamAccessId AccessId,
    string Reason,
    Timestamp RestrictedAt) : DomainEvent;
