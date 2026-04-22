using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.TrustSystem.Access.Session;

public sealed record SessionTerminatedEvent(SessionId SessionId) : DomainEvent;
