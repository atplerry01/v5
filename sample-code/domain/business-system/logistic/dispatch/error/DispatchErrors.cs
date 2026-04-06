namespace Whycespace.Domain.BusinessSystem.Logistic.Dispatch;

public static class DispatchErrors
{
    public const string InvalidTransition = "DISPATCH_INVALID_TRANSITION";
    public const string AlreadyCompleted = "DISPATCH_ALREADY_COMPLETED";
    public const string AlreadyCancelled = "DISPATCH_ALREADY_CANCELLED";
    public const string NotAssigned = "DISPATCH_NOT_ASSIGNED";
    public const string NotInProgress = "DISPATCH_NOT_IN_PROGRESS";
}
