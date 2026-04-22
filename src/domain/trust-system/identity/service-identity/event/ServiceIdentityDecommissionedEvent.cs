using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.TrustSystem.Identity.ServiceIdentity;

public sealed record ServiceIdentityDecommissionedEvent(ServiceIdentityId ServiceIdentityId) : DomainEvent;
