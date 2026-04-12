namespace Whycespace.Domain.BusinessSystem.Portfolio.Rebalance;

public static class RebalanceErrors
{
    public static RebalanceDomainException MissingId()
        => new("RebalanceId is required and must not be empty.");

    public static RebalanceDomainException InvalidStateTransition(RebalanceStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static RebalanceDomainException NameRequired()
        => new("Rebalance must have a name.");

    public static RebalanceDomainException AlreadyCancelled()
        => new("Rebalance has been cancelled and cannot be modified.");
}

public sealed class RebalanceDomainException : Exception
{
    public RebalanceDomainException(string message) : base(message) { }
}
