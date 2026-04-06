namespace Whycespace.Domain.ConstitutionalSystem.Policy.Version;

using Whycespace.Domain.SharedKernel;

public sealed record PolicyVersionActivatedEvent(
    Guid VersionId,
    Guid PolicyRuleId,
    int VersionNumber) : DomainEvent;
