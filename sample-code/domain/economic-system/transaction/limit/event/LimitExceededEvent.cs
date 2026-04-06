namespace Whycespace.Domain.EconomicSystem.Transaction.Limit;

public sealed record LimitExceededEvent(
    Guid LimitId,
    Guid IdentityId,
    string LimitType,
    decimal AttemptedAmount,
    decimal CurrentLimit) : DomainEvent;
