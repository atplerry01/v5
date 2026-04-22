using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.TrustSystem.Identity.Registry;

public sealed record RegistrationInitiatedEvent(
    RegistryId RegistryId,
    RegistrationDescriptor Descriptor,
    Timestamp InitiatedAt) : DomainEvent;
