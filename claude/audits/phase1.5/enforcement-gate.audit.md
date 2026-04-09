# Phase 1.5 — Enforcement Gate Validation Audit (HC-8)

**STATUS: PASS**
**SCOPE: §2.5 of phase1.5-final closure prompt**
**DATE: 2026-04-09**

## Finding

`RuntimeEnforcementGate.Evaluate(...)` is a pure function with a single fixed rule order. `RuntimeControlPlane` invokes it after the HC-7 degraded stamp and before the middleware pipeline. All decisions are deterministic, return structured `CommandResult.Failure(reason)` on a block, and never throw.

## Rule (locked) — [src/shared/contracts/infrastructure/health/RuntimeEnforcementGate.cs](src/shared/contracts/infrastructure/health/RuntimeEnforcementGate.cs)

```
1. maintenance.IsMaintenance                                       → BlockMaintenance ("system_maintenance_mode")
2. degraded.IsDegraded && command is IRestrictedDuringDegraded     → BlockRestricted  ("restricted_during_degraded_mode")
3. degraded.IsDegraded                                             → ProceedRestricted (no reason; soft tag only)
4. otherwise                                                       → Proceed
```
First-match-wins. Maintenance dominates absolutely — even a restricted-during-degraded command sees the maintenance reason, locked by `RuntimeEnforcementGateTests.Maintenance_DominatesDegraded_RestrictedCommand`.

## Position in `ExecuteAsync`

```
acquire MI-1 lock
HSID prelude
stamp DegradedMode (HC-7)
↓
RuntimeEnforcementGate.Evaluate(maintenance, degraded, command)   ← HC-8, BEFORE pipeline
↓
middleware pipeline
```
Block decisions return BEFORE validation, policy, idempotency, or any engine work. Verified by reading [src/runtime/control-plane/RuntimeControlPlane.cs](src/runtime/control-plane/RuntimeControlPlane.cs).

## CommandContext.IsExecutionRestricted

Defined in [src/shared/contracts/runtime/CommandContext.cs](src/shared/contracts/runtime/CommandContext.cs) as a write-once `bool` mirroring the existing `Hsid` / `DegradedMode` write-once pattern. The setter throws `InvalidOperationException` on a second write. Stamped exactly once in the soft-tag branch via `if (!context.IsExecutionRestricted) context.IsExecutionRestricted = true;` — locking is preserved.

## Test coverage

[tests/unit/runtime/RuntimeEnforcementGateTests.cs](tests/unit/runtime/RuntimeEnforcementGateTests.cs) (6 scenarios):
- `Maintenance_HardBlocks_AnyCommand`
- `Maintenance_DominatesDegraded_RestrictedCommand`
- `Degraded_NormalCommand_ProceedsRestricted_NoBlock`
- `Degraded_RestrictedCommand_HardBlocks`
- `Normal_ProceedsCleanly`
- `Normal_RestrictedCommand_NotInDegraded_ProceedsCleanly`

## No policy bypass

The enforcement gate runs BEFORE the policy middleware. A `BlockMaintenance` or `BlockRestricted` decision returns failure WITHOUT setting `PolicyDecisionAllowed`. The `DispatchWithPolicyGuard` defense-in-depth guard would also reject the command at dispatch time if anything bypassed it. There is no path that produces a successful policy-allowed result without a real policy evaluation.

## Result

PASS. Rule order is locked by test, the gate runs before the pipeline, all blocks are exception-free, and no policy bypass exists.
