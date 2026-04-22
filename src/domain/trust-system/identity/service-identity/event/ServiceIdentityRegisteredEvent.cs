using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.TrustSystem.Identity.ServiceIdentity;

public sealed record ServiceIdentityRegisteredEvent(ServiceIdentityId ServiceIdentityId, ServiceIdentityDescriptor Descriptor, Timestamp RegisteredAt) : DomainEvent;
