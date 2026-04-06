namespace Whycespace.Runtime.GuardExecution.Engine;

/// <summary>
/// Thrown when a command attempts to execute without passing guard validation.
/// Non-bypassable — indicates a pipeline misconfiguration or security violation.
/// </summary>
public sealed class ExecutionBypassException : Exception
{
    public ExecutionBypassException(string message)
        : base(message)
    {
    }
}
