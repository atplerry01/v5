namespace Whycespace.Runtime.GuardExecution.Engine;

/// <summary>
/// Thrown when enforcement chain anchoring fails.
/// This is a BLOCKING exception — execution MUST NOT proceed without chain record.
/// </summary>
public sealed class EnforcementChainException : Exception
{
    public EnforcementChainException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
