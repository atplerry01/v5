namespace Whycespace.Domain.BusinessSystem.Portfolio.Performance;

public static class PerformanceErrors
{
    public static PerformanceDomainException MissingId()
        => new("PerformanceId is required and must not be empty.");

    public static PerformanceDomainException InvalidStateTransition(PerformanceStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static PerformanceDomainException NameRequired()
        => new("Performance must have a name.");

    public static PerformanceDomainException AlreadyClosed()
        => new("Performance has been closed and cannot be modified.");
}

public sealed class PerformanceDomainException : Exception
{
    public PerformanceDomainException(string message) : base(message) { }
}
