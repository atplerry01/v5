namespace Whycespace.Platform.Api.Business.Agreement.Clause;

public sealed record ClauseRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record ClauseResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
