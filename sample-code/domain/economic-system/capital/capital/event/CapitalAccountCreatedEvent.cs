using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Capital;

public sealed record CapitalAccountCreatedEvent(Guid CapitalAccountId) : DomainEvent;
