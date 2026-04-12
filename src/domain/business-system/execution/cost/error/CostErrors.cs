namespace Whycespace.Domain.BusinessSystem.Execution.Cost;

public static class CostErrors
{
    public static CostDomainException MissingId()
        => new("CostId is required and must not be empty.");

    public static CostDomainException MissingContextId()
        => new("CostContextId is required and must not be empty.");

    public static CostDomainException ComponentRequired()
        => new("Cost must contain at least one cost component.");

    public static CostDomainException InvalidStateTransition(CostStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static CostDomainException AlreadyCalculated(CostId id)
        => new($"Cost '{id.Value}' has already been calculated.");

    public static CostDomainException AlreadyFinalized(CostId id)
        => new($"Cost '{id.Value}' has already been finalized.");
}

public sealed class CostDomainException : Exception
{
    public CostDomainException(string message) : base(message) { }
}
