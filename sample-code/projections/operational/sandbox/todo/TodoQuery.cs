using Whycespace.Shared.Contracts.Infrastructure;

namespace Whycespace.Projections.Operational.Sandbox.Todo;

public sealed class TodoQuery
{
    private const string Projection = "todo";
    private readonly IProjectionStore _store;

    public TodoQuery(IProjectionStore store)
    {
        _store = store;
    }

    public Task<TodoReadModel?> GetByIdAsync(string todoId, CancellationToken ct = default)
        => _store.GetAsync<TodoReadModel>(Projection, todoId, ct);

    public Task<IReadOnlyList<TodoReadModel>> ListAsync(CancellationToken ct = default)
        => _store.GetAllAsync<TodoReadModel>(Projection, ct);
}
