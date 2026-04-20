using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.Access;

public sealed record StreamAccessExpiredEvent(
    StreamAccessId AccessId,
    Timestamp ExpiredAt) : DomainEvent;
