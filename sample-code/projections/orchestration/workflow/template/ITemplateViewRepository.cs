namespace Whycespace.Projections.Orchestration.Workflow.Template;

public interface ITemplateViewRepository
{
    Task SaveAsync(TemplateReadModel model, CancellationToken ct = default);
    Task<TemplateReadModel?> GetAsync(string id, CancellationToken ct = default);
}
