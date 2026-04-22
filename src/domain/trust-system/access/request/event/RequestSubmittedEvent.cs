using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.TrustSystem.Access.Request;

public sealed record RequestSubmittedEvent(RequestId RequestId, RequestDescriptor Descriptor, Timestamp SubmittedAt) : DomainEvent;
