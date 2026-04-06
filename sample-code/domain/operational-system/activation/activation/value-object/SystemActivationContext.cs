namespace Whycespace.Domain.OperationalSystem.Activation.Activation;

public sealed class SystemActivationContext
{
    public string Mode { get; }
    public bool IsAutonomyAllowed { get; }
    public decimal TransactionLimit { get; }

    public SystemActivationContext(
        string mode,
        bool autonomyAllowed,
        decimal transactionLimit)
    {
        Mode = mode;
        IsAutonomyAllowed = autonomyAllowed;
        TransactionLimit = transactionLimit;
    }

    public bool IsSimulation => Mode == "SIMULATION";
    public bool IsSandbox => Mode == "SANDBOX";
    public bool IsLimitedProduction => Mode == "LIMITED_PRODUCTION";
    public bool IsFullProduction => Mode == "FULL_PRODUCTION";
}
