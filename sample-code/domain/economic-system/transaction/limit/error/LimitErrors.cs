namespace Whycespace.Domain.EconomicSystem.Transaction.Limit;

public static class LimitErrors
{
    public static DomainException TransactionLimitExceeded(Guid identityId, decimal attempted, decimal max) =>
        new("LIMIT_TRANSACTION_EXCEEDED", $"Identity {identityId} exceeded max transaction limit: {attempted} > {max}.");

    public static DomainException DailyLimitExceeded(Guid identityId, decimal dailyTotal, decimal limit) =>
        new("LIMIT_DAILY_EXCEEDED", $"Identity {identityId} exceeded daily limit: {dailyTotal} > {limit}.");

    public static DomainException MonthlyLimitExceeded(Guid identityId, decimal monthlyTotal, decimal limit) =>
        new("LIMIT_MONTHLY_EXCEEDED", $"Identity {identityId} exceeded monthly limit: {monthlyTotal} > {limit}.");
}
