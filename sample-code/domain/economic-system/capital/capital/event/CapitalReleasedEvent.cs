using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Capital;

public sealed record CapitalReleasedEvent(Guid CapitalAccountId, Guid ReleaseId) : DomainEvent;
