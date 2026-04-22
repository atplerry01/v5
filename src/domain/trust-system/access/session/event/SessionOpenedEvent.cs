using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.TrustSystem.Access.Session;

public sealed record SessionOpenedEvent(SessionId SessionId, SessionDescriptor Descriptor, Timestamp OpenedAt) : DomainEvent;
