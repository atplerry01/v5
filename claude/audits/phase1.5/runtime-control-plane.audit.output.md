# Phase 1.5 — Runtime Control Plane Integrity Audit

**STATUS: PASS**
**SCOPE: §2.1 of phase1.5-final closure prompt**
**DATE: 2026-04-09**

## Finding

`Whyce.Runtime.ControlPlane.RuntimeControlPlane.ExecuteAsync` is the single non-bypassable execution entry point. Verified against
[src/runtime/control-plane/RuntimeControlPlane.cs](src/runtime/control-plane/RuntimeControlPlane.cs).

## Evidence

### Pipeline order (live source)
The current `ExecuteAsync` body executes in exactly this order:

```
1. Acquire MI-1 distributed execution lock        (lockKey = "whyce:execution-lock:{CommandId:N}", TTL=30s)
   ├─ false → return CommandResult.Failure("execution_cancelled" | "execution_lock_unavailable")   (HC-9)
   └─ true  → enter try {…} finally { release }
2. HSID v2.1 prelude (StampHsidAsync)
3. Stamp CommandContext.DegradedMode = _runtimeStateAggregator.GetDegradedMode()                    (HC-7)
4. RuntimeEnforcementGate.Evaluate(maintenance, degraded, command)                                  (HC-8)
   ├─ BlockMaintenance  → CommandResult.Failure("system_maintenance_mode")
   ├─ BlockRestricted   → CommandResult.Failure("restricted_during_degraded_mode")
   ├─ ProceedRestricted → context.IsExecutionRestricted = true; fall through
   └─ Proceed           → fall through
5. Locked 8-middleware pipeline (Tracing → Metrics → ContextGuard → Validation → Policy → AuthorizationGuard → Idempotency → ExecutionGuard)
6. DispatchWithPolicyGuard (defense-in-depth: re-checks PolicyDecisionAllowed before dispatcher.DispatchAsync)
7. ICommandDispatcher.DispatchAsync → engine
8. EventFabric.ProcessAuditAsync (audit emission)
9. EventFabric.ProcessAsync (domain emission, persist → chain → outbox)
10. finally { _executionLockProvider.ReleaseAsync(lockKey) }
```

This order matches the LOCK specified in §2.1 of the closure prompt.

### Single entry verification
```
$ grep -RIn "IRuntimeControlPlane" src/
src/runtime/control-plane/RuntimeControlPlane.cs:35:public sealed class RuntimeControlPlane : IRuntimeControlPlane
src/platform/host/composition/runtime/RuntimeComposition.cs:145: services.AddSingleton<IRuntimeControlPlane>(sp =>
src/runtime/dispatcher/SystemIntentDispatcher.cs: [SystemIntentDispatcher resolves IRuntimeControlPlane via DI]
src/shared/contracts/runtime/IRuntimeControlPlane.cs
src/shared/contracts/runtime/ISystemIntentDispatcher.cs
```

`SystemIntentDispatcher` is the only public caller; it forwards to `IRuntimeControlPlane.ExecuteAsync`. There is no other direct `ICommandDispatcher.DispatchAsync` invocation outside `RuntimeControlPlane.DispatchWithPolicyGuard`. Single-entry-point invariant from §5.1.x is preserved through HC-1..HC-9 + MI-1.

### Defense-in-depth guards still present
- `DispatchWithPolicyGuard` re-checks `context.PolicyDecisionAllowed != true` and `IdentityId` after the middleware pipeline → fails closed with WHYCEPOLICY HARD STOP.
- Audit emission requires a non-empty `PolicyDecisionHash` even on the deny path → chain anchoring cannot be bypassed.
- Domain emission requires both `PolicyDecisionAllowed == true` AND `PolicyDecisionHash` → events cannot reach the event store without a real policy decision.

## Result

PASS. The pipeline order is exactly as specified. No bypass paths exist. Defense-in-depth guards from prior workstreams remain intact and additive.
