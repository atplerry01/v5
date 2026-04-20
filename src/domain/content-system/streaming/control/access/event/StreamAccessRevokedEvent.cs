using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.Control.Access;

public sealed record StreamAccessRevokedEvent(
    StreamAccessId AccessId,
    string Reason,
    Timestamp RevokedAt) : DomainEvent;
