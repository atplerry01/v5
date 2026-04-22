using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.TrustSystem.Identity.Registry;

public sealed record RegistrationLockedEvent(RegistryId RegistryId, string Reason) : DomainEvent;
