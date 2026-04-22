using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.TrustSystem.Access.Request;

public sealed record RequestWithdrawnEvent(RequestId RequestId) : DomainEvent;
