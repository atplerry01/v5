using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.OperationalSystem.Deployment.Sandbox;

public sealed class SandboxCapitalCapExceededException : DomainException
{
    public SandboxCapitalCapExceededException(decimal cap, decimal attempted)
        : base("SANDBOX_CAPITAL_CAP", $"Transaction would exceed sandbox capital cap of {cap}. Attempted: {attempted}.") { }
}

public sealed class SandboxTransactionLimitException : DomainException
{
    public SandboxTransactionLimitException(int limit)
        : base("SANDBOX_TRANSACTION_LIMIT", $"Sandbox transaction limit of {limit} reached.") { }
}
