namespace Whycespace.Projections.Intelligence.Knowledge.Article;

public sealed record ArticleView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
