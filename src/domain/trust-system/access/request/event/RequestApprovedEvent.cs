using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.TrustSystem.Access.Request;

public sealed record RequestApprovedEvent(RequestId RequestId) : DomainEvent;
