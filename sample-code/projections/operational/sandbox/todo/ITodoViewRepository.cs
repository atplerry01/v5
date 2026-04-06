namespace Whycespace.Projections.Operational.Sandbox.Todo;

public interface ITodoViewRepository
{
    Task SaveAsync(TodoReadModel model, CancellationToken ct = default);
    Task<TodoReadModel?> GetAsync(string id, CancellationToken ct = default);
}
