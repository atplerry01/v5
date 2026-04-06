using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.ConstitutionalSystem.Policy.Rule;

public sealed record PolicyVersionSetEvent(
    Guid PolicyId,
    Guid VersionId) : DomainEvent;
