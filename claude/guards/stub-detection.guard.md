# STUB DETECTION GUARD

**Status:** ACTIVE
**Severity baseline:** S0 fails build; S1 requires explicit registry entry.
**Owner:** WBSM v3 structural integrity.

## SCOPE
All files under `src/`. Tests are out of scope.

## RULES

### STUB-R1 — Zero NotImplementedException on production path (S0)
`throw new NotImplementedException(...)` is FORBIDDEN in:
- `src/domain/**`
- `src/engines/**`
- `src/runtime/**`
- `src/platform/api/**`
- Anywhere on the Todo E2E path: API → Runtime → Engines → Persistence → Kafka → Response

If a method must be unimplemented, throw a structured domain exception with explicit reason, OR remove the method.

### STUB-R2 — Zero TODO/FIXME/HACK on production path (S1)
Comments containing `TODO`, `FIXME`, `HACK`, `XXX` are forbidden in production code. Convert to GitHub issues or new-rules entries instead.

### STUB-R3 — Placeholder implementations must be registered (S1)
A class is a "tracked placeholder" only if:
1. Class name begins with `InMemory` OR file/class XML doc contains the literal token `PLACEHOLDER (T-PLACEHOLDER-NN)`
2. There is a corresponding registry entry in `claude/registry/placeholders.json` (or equivalent) with:
   - `id` (matching `T-PLACEHOLDER-NN`)
   - `file`
   - `replacement_target` (e.g., the migration script or canonical implementation path)
   - `phase_gate` (which phase must replace it)
3. Architecture test enforces 1:1 between marker and registry.

Untracked placeholders are S1 violations.

### STUB-R4 — No silent exception swallowing (S2)
Forbidden:
```csharp
catch { }
catch (Exception) { }
```
Allowed:
- `catch (OperationCanceledException) { return; }` in shutdown paths
- `catch (SpecificException ex) { _logger.LogDebug(ex, "..."); /* known recoverable */ }`

### STUB-R5 — No empty interface implementations without doc (S2)
An empty `void` method implementing an interface contract requires an XML doc comment explaining why it is intentionally a no-op (e.g., "schema-only module owns no engines").

### STUB-R6 — No hardcoded placeholder return values (S2)
`return true;`, `return 0;`, `return "ok";`, `return new List<T>();` as the entire method body is forbidden unless the method's contract permits it (verified by name like `IsAlwaysTrue` or interface explicitly states "returns empty when …").

## CI ENFORCEMENT
1. **Architecture test:** grep `src/{domain,engines,runtime,platform/api}/**/*.cs` for `NotImplementedException` — fail.
2. **Architecture test:** grep `src/**/*.cs` for `\bTODO\b|\bFIXME\b|\bHACK\b|\bXXX\b` outside `// XML doc` — fail.
3. **Architecture test:** for every class matching `^InMemory.*` or comment `PLACEHOLDER \(T-PLACEHOLDER-\d+\)`, assert a registry entry exists in `claude/registry/placeholders.json`.
4. **Architecture test:** scan for `catch\s*\{\s*\}` — fail.

## CURRENTLY TRACKED PLACEHOLDERS
- `T-PLACEHOLDER-01` — InMemoryWorkflowExecutionProjectionStore (replaced by Postgres impl after migration 002)
- `T-PLACEHOLDER-02` — InMemoryStructureRegistry (replaced by canonical constitutional registry)

(Both must be added to `claude/registry/placeholders.json` when that registry is created — see new-rules entry 20260408-132840-stub-detection.md.)
