namespace Whycespace.Engines.T0U.WhyceId.Audit;

public sealed class AuditHandler : IAuditEngine
{
    private static readonly HashSet<string> HighAuditActions = new(StringComparer.OrdinalIgnoreCase)
    {
        "Deactivate", "Decommission", "Block", "Suspend", "RevokeCredential"
    };

    public AuditDecisionResult Evaluate(AuditDecisionCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (HighAuditActions.Contains(command.Action))
            return AuditDecisionResult.Required("High");

        return AuditDecisionResult.Required("Standard");
    }
}
