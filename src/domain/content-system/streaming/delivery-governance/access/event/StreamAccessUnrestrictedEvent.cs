using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.Access;

public sealed record StreamAccessUnrestrictedEvent(
    StreamAccessId AccessId,
    Timestamp UnrestrictedAt) : DomainEvent;
