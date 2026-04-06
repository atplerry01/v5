namespace Whycespace.Platform.Api.Intelligence.Knowledge.Answer;

public sealed record AnswerRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record AnswerResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
