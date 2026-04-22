namespace Whycespace.Domain.ControlSystem.SystemPolicy.PolicyAudit;

public enum PolicyAuditCategory
{
    EvaluationPass = 1,
    EvaluationDeny = 2,
    EvaluationDefer = 3,
    PackageDeployment = 4,
    PackageRetirement = 5,
    PolicyDefinitionChange = 6
}
