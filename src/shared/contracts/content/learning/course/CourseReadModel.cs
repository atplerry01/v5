namespace Whycespace.Shared.Contracts.Content.Learning.Course;

public sealed record CourseReadModel
{
    public Guid Id { get; init; }
    public string OwnerRef { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Status { get; init; } = "draft";
    public DateTimeOffset? DraftedAt { get; init; }
    public DateTimeOffset? LastTransitionedAt { get; init; }
    public IReadOnlyDictionary<string, int> Outline { get; init; } =
        new Dictionary<string, int>(StringComparer.Ordinal);
}
