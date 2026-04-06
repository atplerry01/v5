namespace Whycespace.Platform.Api.Decision.Governance.Exception;

public sealed record ExceptionRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record ExceptionResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
