using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.Access;

public sealed record StreamAccessGrantedEvent(
    StreamAccessId AccessId,
    StreamRef StreamRef,
    AccessMode Mode,
    AccessWindow Window,
    TokenBinding Token,
    Timestamp GrantedAt) : DomainEvent;
