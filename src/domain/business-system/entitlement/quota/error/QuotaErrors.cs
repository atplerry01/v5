namespace Whycespace.Domain.BusinessSystem.Entitlement.Quota;

public static class QuotaErrors
{
    public static QuotaDomainException MissingId()
        => new("QuotaId is required and must not be empty.");

    public static QuotaDomainException MissingSubjectId()
        => new("QuotaSubjectId is required and must not be empty.");

    public static QuotaDomainException InvalidCapacity()
        => new("TotalCapacity must be greater than zero.");

    public static QuotaDomainException InvalidStateTransition(QuotaStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static QuotaDomainException CapacityExceeded(int requested, int remaining)
        => new($"Cannot consume {requested} units. Only {remaining} remaining.");

    public static QuotaDomainException CapacityRemaining(int remaining)
        => new($"Cannot exhaust quota. {remaining} units of capacity remain.");

    public static QuotaDomainException ConsumptionExceedsCapacity(int totalConsumed, int totalCapacity)
        => new($"Total consumed ({totalConsumed}) exceeds total capacity ({totalCapacity}).");

    public static QuotaDomainException AlreadyExhausted(QuotaId id)
        => new($"Quota '{id.Value}' has already been exhausted.");
}

public sealed class QuotaDomainException : Exception
{
    public QuotaDomainException(string message) : base(message) { }
}
