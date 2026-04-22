using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.TrustSystem.Identity.Consent;

public sealed record ConsentGrantedEvent(ConsentId ConsentId, ConsentDescriptor Descriptor, Timestamp GrantedAt) : DomainEvent;
