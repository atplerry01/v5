---
name: composition-loader
type: structural
severity: S1
locked: true
---

# Composition Loader Guard

## Purpose
Lock the deterministic composition module loader. The loader must remain the
sole entry point for category composition wiring inside `Program.cs`, and the
`CompositionRegistry` must remain explicit, ordered, and reflection-free.

## Rules

### G-COMPLOAD-01 — Registry Membership
FAIL IF any class implementing `ICompositionModule` is not listed in
`src/platform/host/composition/registry/CompositionRegistry.cs`.

### G-COMPLOAD-02 — Explicit Order
FAIL IF any `ICompositionModule` implementation does not define a unique,
non-negative integer `Order`. Duplicate or missing `Order` values are S1.

### G-COMPLOAD-03 — Locked Execution Sequence
FAIL IF the registry order deviates from:
`Core(0) → Runtime(1) → Infrastructure(2) → Projections(3) → Observability(4)`.
Adding a new module requires extending this sequence and updating this guard.

### G-COMPLOAD-04 — Loader-Only Composition
FAIL IF `Program.cs` re-introduces direct `Add*Composition(...)` calls instead
of `builder.Services.LoadModules(builder.Configuration)`.

### G-COMPLOAD-05 — BootstrapModuleCatalog Preserved
FAIL IF the `BootstrapModuleCatalog.All` registration loop is removed from
`Program.cs` or migrated into the composition loader. Domain bootstrap MUST
remain a separate, explicit pass.

### G-COMPLOAD-06 — No Reflection Discovery
FAIL IF the loader, registry, or any composition module discovers types via
reflection (`Assembly.GetTypes`, `Activator.CreateInstance`, attribute scans,
etc.). Module enumeration is explicit list literals only.

### G-COMPLOAD-07 — Modules Are Orchestration-Only
FAIL IF any `ICompositionModule.Register` body contains anything beyond a
single delegating call to its category `Add*Composition` extension. No `new`,
no `services.AddSingleton<...>` calls inside modules themselves.

## Severity
- All G-COMPLOAD-* rules: **S1 — HIGH** (block merge)
