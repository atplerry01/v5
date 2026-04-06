namespace Whycespace.Runtime.GuardExecution.Engine;

/// <summary>
/// S0 = FAIL IMMEDIATELY, S1 = FAIL, S2 = WARN, S3 = LOG
/// </summary>
public enum GuardSeverity
{
    S0 = 0,
    S1 = 1,
    S2 = 2,
    S3 = 3
}
