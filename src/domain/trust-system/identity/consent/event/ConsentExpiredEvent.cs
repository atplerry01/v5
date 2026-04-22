using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.TrustSystem.Identity.Consent;

public sealed record ConsentExpiredEvent(ConsentId ConsentId) : DomainEvent;
