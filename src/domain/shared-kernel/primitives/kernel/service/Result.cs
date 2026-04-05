namespace Whycespace.Domain.SharedKernel.Primitives.Kernel;

public sealed record Result<T>
{
    public T? Value { get; init; }
    public string? Error { get; init; }
    public bool IsSuccess => Error is null;
}
