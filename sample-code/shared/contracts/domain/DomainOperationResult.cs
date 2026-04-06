namespace Whycespace.Shared.Contracts.Domain;

public sealed record DomainOperationResult
{
    public required bool Success { get; init; }
    public Guid? AggregateId { get; init; }
    public object? Data { get; init; }
    public string? ErrorMessage { get; init; }
    public string? ErrorCode { get; init; }

    public static DomainOperationResult Ok(Guid? aggregateId = null, object? data = null) => new()
    {
        Success = true,
        AggregateId = aggregateId,
        Data = data
    };

    public static DomainOperationResult Fail(string errorMessage, string? errorCode = null) => new()
    {
        Success = false,
        ErrorMessage = errorMessage,
        ErrorCode = errorCode
    };
}
