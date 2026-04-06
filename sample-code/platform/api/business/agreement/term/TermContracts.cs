namespace Whycespace.Platform.Api.Business.Agreement.Term;

public sealed record TermRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record TermResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
