namespace Whycespace.Engines.T2E.Economic.Enforcement.Enforcement;

public enum EnforcementDecision { Allow, Deny, Conditional }

public record EnforcementResult(bool Success, string Message);

public sealed record EnforcementDto(
    string EnforcementId,
    string IdentityId,
    string EnforcementType,
    string Scope,
    string Duration,
    string Status,
    EnforcementDecision Decision,
    string? ReasonCode);
