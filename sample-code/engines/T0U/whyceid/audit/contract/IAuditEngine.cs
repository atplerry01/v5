namespace Whycespace.Engines.T0U.WhyceId.Audit;

public interface IAuditEngine
{
    AuditDecisionResult Evaluate(AuditDecisionCommand command);
}

public sealed record AuditDecisionCommand(
    string ActorId,
    string Action,
    string Resource,
    string Reason);

public sealed record AuditDecisionResult(
    bool RequiresAudit,
    string AuditLevel,
    string? Reason = null)
{
    public static AuditDecisionResult Required(string level) => new(true, level);
    public static AuditDecisionResult NotRequired() => new(false, "None");
}
