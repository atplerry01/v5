using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.TrustSystem.Identity.ServiceIdentity;

public sealed record ServiceIdentitySuspendedEvent(ServiceIdentityId ServiceIdentityId) : DomainEvent;
