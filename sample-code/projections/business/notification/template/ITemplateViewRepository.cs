namespace Whycespace.Projections.Business.Notification.Template;

public interface ITemplateViewRepository
{
    Task SaveAsync(TemplateReadModel model, CancellationToken ct = default);
    Task<TemplateReadModel?> GetAsync(string id, CancellationToken ct = default);
}
