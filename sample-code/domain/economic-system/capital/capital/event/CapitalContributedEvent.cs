using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Capital;

public sealed record CapitalContributedEvent(Guid CapitalAccountId, Guid ContributionId, Guid ContributorIdentityId) : DomainEvent;
