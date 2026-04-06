using Whycespace.Shared.Contracts.Projection;

namespace Whycespace.Projections.Intelligence.Knowledge.Article;

public sealed class ArticleProjectionHandler
{
    public string ProjectionName => "whyce.intelligence.knowledge.article";

    public string[] EventTypes =>
    [
        "whyce.intelligence.knowledge.article.created",
        "whyce.intelligence.knowledge.article.updated"
    ];

    public async Task HandleAsync(ProjectionEvent @event, IArticleViewRepository repository, CancellationToken ct)
    {
        // Build/update read model from event
        var model = new ArticleReadModel
        {
            Id = @event.AggregateId.ToString(),
            Status = "Active",
            LastUpdated = @event.Timestamp,
            LastEventTimestamp = @event.Timestamp,
            LastEventVersion = @event.Version
        };

        await repository.SaveAsync(model, ct);
    }
}
