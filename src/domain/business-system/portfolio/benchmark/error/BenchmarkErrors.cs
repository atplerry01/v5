namespace Whycespace.Domain.BusinessSystem.Portfolio.Benchmark;

public static class BenchmarkErrors
{
    public static BenchmarkDomainException MissingId()
        => new("BenchmarkId is required and must not be empty.");

    public static BenchmarkDomainException InvalidStateTransition(BenchmarkStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");

    public static BenchmarkDomainException NameRequired()
        => new("Benchmark must have a name.");

    public static BenchmarkDomainException AlreadyRetired()
        => new("Benchmark has been retired and cannot be modified.");
}

public sealed class BenchmarkDomainException : Exception
{
    public BenchmarkDomainException(string message) : base(message) { }
}
