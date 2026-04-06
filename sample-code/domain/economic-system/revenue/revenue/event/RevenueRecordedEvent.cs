using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.EconomicSystem.Revenue.Revenue;

public sealed record RevenueRecordedEvent(
    Guid RevenueAccountId,
    decimal Amount,
    string CurrencyCode) : DomainEvent;
