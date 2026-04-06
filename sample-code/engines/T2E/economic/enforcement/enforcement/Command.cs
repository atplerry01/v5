namespace Whycespace.Engines.T2E.Economic.Enforcement.Enforcement;

public record EnforcementCommand(string Action, string EntityId, object Payload);

public sealed record ApplyEnforcementCommand(
    string EnforcementId,
    string IdentityId,
    string EnforcementType,
    string Scope,
    string Duration,
    string Reason) : EnforcementCommand("Apply", EnforcementId, null!);

public sealed record ReleaseEnforcementCommand(string EnforcementId)
    : EnforcementCommand("Release", EnforcementId, null!);
