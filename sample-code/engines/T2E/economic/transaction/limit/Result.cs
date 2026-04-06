namespace Whycespace.Engines.T2E.Economic.Transaction.Limit;

public enum LimitDecision { Allow, Deny, Conditional }

public record LimitResult(bool Success, string Message);

public sealed record LimitEvaluationDto(
    string LimitId,
    string IdentityId,
    LimitDecision Decision,
    string? ReasonCode,
    string? ViolationType);
