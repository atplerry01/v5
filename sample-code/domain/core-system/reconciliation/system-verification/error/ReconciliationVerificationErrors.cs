using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.CoreSystem.Reconciliation.SystemVerification;

public sealed class VerificationSessionTerminalException : DomainException
{
    public VerificationSessionTerminalException(string status)
        : base("VERIFICATION_SESSION_TERMINAL", $"Verification session is in terminal state: {status}.") { }
}

public sealed class ConsistencyCheckFailedException : DomainException
{
    public ConsistencyCheckFailedException(string checkName, string details)
        : base("CONSISTENCY_CHECK_FAILED", $"Consistency check '{checkName}' failed: {details}") { }
}
