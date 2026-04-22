using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.TrustSystem.Access.Grant;

public sealed record GrantIssuedEvent(GrantId GrantId, GrantDescriptor Descriptor, Timestamp IssuedAt) : DomainEvent;
