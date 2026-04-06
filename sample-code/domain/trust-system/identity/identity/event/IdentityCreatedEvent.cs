using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.TrustSystem.Identity.Identity;

public sealed record IdentityCreatedEvent(Guid IdentityId) : DomainEvent;
