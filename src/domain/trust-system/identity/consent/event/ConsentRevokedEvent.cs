using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.TrustSystem.Identity.Consent;

public sealed record ConsentRevokedEvent(ConsentId ConsentId) : DomainEvent;
