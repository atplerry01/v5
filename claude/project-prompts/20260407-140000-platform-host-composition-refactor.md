---
classification: platform / host / composition-refactor
context: src/platform/host
domain: composition modules
mode: SAFE PATCH (zero behavior change)
priority: HIGH
date: 2026-04-07
---

# TITLE
Whycespace — Program.cs Refactor to Composition Modules (Safe Patch)

# CONTEXT
`src/platform/host/Program.cs` previously contained ~300 lines mixing core
primitives, runtime engine wiring, infrastructure adapter instantiation,
event-fabric/projection wiring, health checks, swagger, and the HTTP pipeline.
Per-domain wiring was already extracted via `BootstrapModuleCatalog` /
`IDomainBootstrapModule` (Phase B2a). The remaining non-domain registrations
needed extraction into category-aligned composition modules.

# OBJECTIVE
Extract all non-pipeline service registration from `Program.cs` into category
composition modules under `src/platform/host/composition/{category}/`,
preserving:
- Same services
- Same registration ordering for `IEnumerable<T>` consumers (health checks)
- Same locked middleware order (enforced by `runtime-order.guard.md`)
- Same HTTP pipeline order
- Same configuration reads with no hardcoded fallbacks

# CONSTRAINTS
- ZERO behavior change
- No domain logic in host
- No inline service registration in Program.cs
- Domain wiring continues through `BootstrapModuleCatalog` (classification-aligned)
- WBSM v3 layer purity preserved
- Existing adapter files in `src/platform/host/adapters/` are NOT moved

# EXECUTION STEPS
1. Read existing `Program.cs` and `BootstrapModuleCatalog` / `IDomainBootstrapModule`.
2. Create composition modules:
   - `composition/core/CoreComposition.cs` (+ `SystemClock`, `DeterministicIdGenerator`)
   - `composition/infrastructure/InfrastructureComposition.cs`
   - `composition/runtime/RuntimeComposition.cs`
   - `composition/projections/ProjectionComposition.cs`
   - `composition/observability/ObservabilityComposition.cs`
3. Rewrite `Program.cs` to call only `Add*Composition` + bootstrap module loop + HTTP pipeline.
4. Add `claude/guards/program-composition.guard.md`.
5. Run `dotnet build`.

# OUTPUT FORMAT
- Files created (list)
- Program.cs before/after line count
- Build result
- Behavior verification statement

# VALIDATION CRITERIA
1. `dotnet build` passes
2. `Program.cs` < 100 non-empty lines
3. No `builder.Services.Add*<T>` in Program.cs (only `Add*Composition`)
4. Locked middleware order untouched in `RuntimeComposition`
5. Health-check registration order preserved in `ObservabilityComposition`
6. All connection-string reads still throw on missing required config
