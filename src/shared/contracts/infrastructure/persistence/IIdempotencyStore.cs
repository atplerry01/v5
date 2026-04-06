namespace Whyce.Shared.Contracts.Infrastructure.Persistence;

public interface IIdempotencyStore
{
    Task<bool> ExistsAsync(string key);
    Task MarkAsync(string key);
}
