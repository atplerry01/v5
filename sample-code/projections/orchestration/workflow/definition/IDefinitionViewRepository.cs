namespace Whycespace.Projections.Orchestration.Workflow.Definition;

public interface IDefinitionViewRepository
{
    Task SaveAsync(DefinitionReadModel model, CancellationToken ct = default);
    Task<DefinitionReadModel?> GetAsync(string id, CancellationToken ct = default);
}
