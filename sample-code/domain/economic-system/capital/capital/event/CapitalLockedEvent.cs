using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Capital;

public sealed record CapitalLockedEvent(Guid CapitalAccountId, Guid LockId) : DomainEvent;
