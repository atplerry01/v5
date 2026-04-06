namespace Whycespace.Runtime.GuardExecution.Runtime;

/// <summary>
/// Classification of enforcement failures for observability and alerting.
/// </summary>
public enum EnforcementFailureType
{
    GuardViolation,
    PolicyDenial,
    ChainFailure,
    RuntimeFailure
}
