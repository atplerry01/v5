namespace Whycespace.Engines.T2E.Economic.Transaction.Limit;

public record LimitCommand(string Action, string EntityId, object Payload);

public sealed record EvaluateLimitCommand(
    string LimitId,
    string IdentityId,
    decimal TransactionAmount,
    decimal MaxTransactionLimit,
    decimal DailyTotal,
    decimal DailyLimit,
    decimal MonthlyTotal,
    decimal MonthlyLimit) : LimitCommand("Evaluate", LimitId, null!);
