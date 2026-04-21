namespace Whycespace.Shared.Contracts.Business.Agreement.ChangeControl.Clause;

public sealed record ClauseReadModel
{
    public Guid ClauseId { get; init; }
    public string ClauseType { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset LastUpdatedAt { get; init; }
}
