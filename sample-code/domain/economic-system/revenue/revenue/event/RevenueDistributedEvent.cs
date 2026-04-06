using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.EconomicSystem.Revenue.Revenue;

public sealed record RevenueDistributedEvent(
    Guid RevenueAccountId,
    decimal Amount,
    string CurrencyCode,
    Guid RecipientIdentityId) : DomainEvent;
