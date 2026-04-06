namespace Whycespace.Domain.EconomicSystem.Transaction.Limit;

public sealed record LimitAssignedEvent(
    Guid LimitId,
    Guid IdentityId,
    decimal MaxTransaction,
    decimal DailyLimit,
    decimal MonthlyLimit) : DomainEvent;
