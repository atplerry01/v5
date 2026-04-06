namespace Whycespace.Domain.ConstitutionalSystem.Policy.Version;

using Whycespace.Domain.SharedKernel;

public sealed record PolicyVersionCreatedEvent(
    Guid VersionId,
    Guid PolicyRuleId,
    int VersionNumber) : DomainEvent;
