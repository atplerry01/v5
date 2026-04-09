# Phase 1.5 — Execution Lock & Multi-Instance Safety Audit

**STATUS: PASS**
**SCOPE: §2.4 of phase1.5-final closure prompt**
**DATE: 2026-04-09**

## Finding

`RedisExecutionLockProvider` implements the canonical SET NX PX + Lua compare-and-delete pattern. The lock wraps the entire `RuntimeControlPlane.ExecuteAsync` body in try/finally so every code path surrenders the lease. Failures map deterministically to `execution_lock_unavailable` / `execution_cancelled` (HC-9 update) — never an exception.

## Evidence

### Acquire path ([src/platform/host/runtime/RedisExecutionLockProvider.cs](src/platform/host/runtime/RedisExecutionLockProvider.cs))
```csharp
var token = NextOwnerToken();   // {MachineName}:{ProcessId}:{Interlocked counter}
try {
    var db = _redis.GetDatabase();
    var ok = await db.StringSetAsync(key, token, ttl, When.NotExists);   // SET NX PX
    if (ok) _owners[key] = token;
    return ok;
} catch {
    return false;   // HC-9: Redis outage collapses to deterministic false
}
```

### Release path (Lua compare-and-delete)
```lua
if redis.call('GET', KEYS[1]) == ARGV[1] then
    return redis.call('DEL', KEYS[1])
else
    return 0
end
```
Owner-safe by construction. A stale process whose lease has expired cannot accidentally unlock a key that has since been re-acquired by another owner. Both `TryAcquireAsync` and `ReleaseAsync` are wrapped in catch-all blocks (HC-9) — no exceptions cross this seam.

### Wrapping in [src/runtime/control-plane/RuntimeControlPlane.cs](src/runtime/control-plane/RuntimeControlPlane.cs)
```
acquire(lockKey, 30s)
↓
try {
    HSID prelude
    Stamp DegradedMode (HC-7)
    Enforcement gate (HC-8)
    Pipeline
    DispatchWithPolicyGuard
    ICommandDispatcher.DispatchAsync
    EventFabric.ProcessAuditAsync
    EventFabric.ProcessAsync
    return result;
} finally {
    await _executionLockProvider.ReleaseAsync(lockKey);
}
```
Lease release is unconditional — exception paths still surrender the lock.

### Failure mapping
```csharp
if (!acquired) {
    if (cancellationToken.IsCancellationRequested)
        return CommandResult.Failure("execution_cancelled");
    return CommandResult.Failure("execution_lock_unavailable");
}
```

### Test coverage
[tests/unit/runtime/ExecutionLockProviderTests.cs](tests/unit/runtime/ExecutionLockProviderTests.cs):
- `SingleAcquire_Succeeds`
- `ConcurrentAcquire_OnlyOneSucceeds` (16 parallel callers → exactly 1 success)
- `ReleaseAfterAcquire_AllowsReacquire`
- `ReleaseUnheldKey_IsNoop` (owner-safe)
- `DistinctKeys_DoNotInterfere`
- `ProviderUnderRedisOutage_ReturnsFalse_NoThrow` (HC-9 contract)

## Result

PASS. Lock semantics, wrapping, owner-safety, and exception-free contract are all verified by source and test.
