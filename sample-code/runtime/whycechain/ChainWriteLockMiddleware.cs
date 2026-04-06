using Whycespace.Runtime.Command;
using Whycespace.Shared.Contracts.Infrastructure.Locking;

namespace Whycespace.Runtime.WhyceChain;

/// <summary>
/// Middleware that acquires a distributed lock for chain write operations.
/// Non-chain commands bypass the lock entirely.
/// If the lock cannot be acquired, returns CHAIN.LOCK_NOT_ACQUIRED without executing.
/// </summary>
public sealed class ChainWriteLockMiddleware
{
    private const string LockKey = "whycechain:write:lock";
    private static readonly TimeSpan LockTtl = TimeSpan.FromSeconds(30);

    private readonly IDistributedLock _lock;

    public ChainWriteLockMiddleware(IDistributedLock distributedLock)
    {
        _lock = distributedLock ?? throw new ArgumentNullException(nameof(distributedLock));
    }

    public async Task<CommandResult> InvokeAsync(
        CommandContext context,
        Func<CommandContext, Task<CommandResult>> next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        if (!IsChainWriteCommand(context))
            return await next(context);

        var acquired = await _lock.AcquireAsync(LockKey, LockTtl, context.CancellationToken);
        if (!acquired)
        {
            return CommandResult.Fail(
                context.Envelope.CommandId,
                "Chain write lock not acquired — concurrent write in progress",
                "CHAIN.LOCK_NOT_ACQUIRED");
        }

        try
        {
            return await next(context);
        }
        finally
        {
            await _lock.ReleaseAsync(LockKey, context.CancellationToken);
        }
    }

    private static bool IsChainWriteCommand(CommandContext context) =>
        context.Envelope.CommandType.Contains("ChainWrite", StringComparison.OrdinalIgnoreCase);
}
