namespace Whyce.Engines.T0U.WhyceChain.Lock;

/// <summary>
/// Chain lock — prevents concurrent modifications to the chain.
/// Ensures sequential anchoring and prevents race conditions.
/// </summary>
public sealed class ChainLock
{
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public async Task<ChainLockHandle> AcquireAsync()
    {
        await _semaphore.WaitAsync();
        return new ChainLockHandle(_semaphore);
    }
}

/// <summary>
/// Disposable lock handle. Releasing the handle releases the chain lock.
/// </summary>
public sealed class ChainLockHandle : IAsyncDisposable
{
    private readonly SemaphoreSlim _semaphore;
    private bool _disposed;

    internal ChainLockHandle(SemaphoreSlim semaphore)
    {
        _semaphore = semaphore;
    }

    public ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            _disposed = true;
            _semaphore.Release();
        }

        return ValueTask.CompletedTask;
    }
}
