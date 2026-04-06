namespace Whycespace.Projections.Business.Execution.Completion;

public interface ICompletionViewRepository
{
    Task SaveAsync(CompletionReadModel model, CancellationToken ct = default);
    Task<CompletionReadModel?> GetAsync(string id, CancellationToken ct = default);
}
