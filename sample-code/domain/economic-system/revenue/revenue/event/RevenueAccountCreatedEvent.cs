using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.EconomicSystem.Revenue.Revenue;

public sealed record RevenueAccountCreatedEvent(Guid RevenueAccountId) : DomainEvent;
