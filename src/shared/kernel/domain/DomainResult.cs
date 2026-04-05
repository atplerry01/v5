namespace Whycespace.Shared.Kernel.Domain;

public sealed class DomainResult
{
    public bool IsSuccess { get; }
    public string? Error { get; }

    private DomainResult(bool isSuccess, string? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static DomainResult Success() => new(true, null);
    public static DomainResult Failure(string error) => new(false, error);
}

public sealed class DomainResult<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? Error { get; }

    private DomainResult(bool isSuccess, T? value, string? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public static DomainResult<T> Success(T value) => new(true, value, null);
    public static DomainResult<T> Failure(string error) => new(false, default, error);
}
