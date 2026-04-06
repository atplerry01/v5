namespace Whycespace.Projections.Intelligence.Knowledge.Answer;

public sealed record AnswerView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
