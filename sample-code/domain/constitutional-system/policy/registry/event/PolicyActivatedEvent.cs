using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.ConstitutionalSystem.Policy.Rule;

public sealed record PolicyActivatedEvent(Guid PolicyId) : DomainEvent;
