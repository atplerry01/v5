namespace Whycespace.Projections.Business.Execution.Completion;

public sealed record CompletionView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
