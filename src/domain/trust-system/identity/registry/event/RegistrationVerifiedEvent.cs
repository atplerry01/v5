using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.TrustSystem.Identity.Registry;

public sealed record RegistrationVerifiedEvent(RegistryId RegistryId) : DomainEvent;
