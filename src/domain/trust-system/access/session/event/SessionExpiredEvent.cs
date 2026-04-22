using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.TrustSystem.Access.Session;

public sealed record SessionExpiredEvent(SessionId SessionId) : DomainEvent;
