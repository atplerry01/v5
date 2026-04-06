using Whycespace.Runtime.Command;

namespace Whycespace.Runtime.Idempotency;

public interface IIdempotencyRegistry
{
    Task<bool> ExistsAsync(Guid commandId, CancellationToken cancellationToken = default);
    Task<CommandResult?> GetResultAsync(Guid commandId, CancellationToken cancellationToken = default);
    Task RegisterAsync(Guid commandId, CommandResult result, CancellationToken cancellationToken = default);

    // String-key overloads for X-Idempotency-Key header support
    Task<CommandResult?> GetResultByKeyAsync(string idempotencyKey, CancellationToken cancellationToken = default);
    Task RegisterByKeyAsync(string idempotencyKey, CommandResult result, CancellationToken cancellationToken = default);
}
