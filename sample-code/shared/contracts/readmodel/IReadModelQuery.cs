namespace Whycespace.Shared.Contracts.ReadModel;

/// <summary>
/// Abstraction for querying read-model projections.
/// Implementations live in infrastructure — consumers never touch persistence directly.
/// </summary>
public interface IReadModelQuery<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<T>> ListAsync(IReadModelFilter? filter = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Optional filter criteria passed to read-model queries.
/// </summary>
public interface IReadModelFilter
{
    IDictionary<string, object> Criteria { get; }
}

public sealed record ReadModelFilter : IReadModelFilter
{
    public IDictionary<string, object> Criteria { get; init; } = new Dictionary<string, object>();
}
