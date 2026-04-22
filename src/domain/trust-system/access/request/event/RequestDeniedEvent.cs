using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.TrustSystem.Access.Request;

public sealed record RequestDeniedEvent(RequestId RequestId, string Reason) : DomainEvent;
