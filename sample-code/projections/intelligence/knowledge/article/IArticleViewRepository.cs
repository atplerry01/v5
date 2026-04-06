namespace Whycespace.Projections.Intelligence.Knowledge.Article;

public interface IArticleViewRepository
{
    Task SaveAsync(ArticleReadModel model, CancellationToken ct = default);
    Task<ArticleReadModel?> GetAsync(string id, CancellationToken ct = default);
}
