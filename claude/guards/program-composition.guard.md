---
name: program-composition
type: structural
severity: S1
locked: true
---

# Program Composition Guard

## Purpose
Lock `src/platform/host/Program.cs` to composition-only orchestration. All
service registration must live inside category composition modules under
`src/platform/host/composition/{category}/`.

## Rules

### G-PROGCOMP-01 — Composition Only
FAIL IF `Program.cs` contains any of:
- `builder.Services.AddSingleton<` / `AddTransient<` / `AddScoped<`
- `builder.Services.AddHostedService(`
- Direct `new` of any infrastructure adapter or middleware
- `Configuration.GetValue<` / `Configuration["..."]` reads

ALLOWED:
- `WebApplication.CreateBuilder` and `builder.Build()`
- Calls to `Add*Composition(...)` extension methods
- Calls to `LoadModules(...)` from `CompositionModuleLoader` (deterministic registry walk)
- Calls to bootstrap module `RegisterServices` from `BootstrapModuleCatalog`
- HTTP pipeline configuration (`app.Use*`, `app.Map*`)
- `app.Run()`

### G-PROGCOMP-02 — Size Cap
`Program.cs` MUST NOT exceed 100 non-empty lines. Re-extract before crossing
this threshold.

### G-PROGCOMP-03 — Classification-Aligned Domain Wiring
Domain registration MUST flow through `IDomainBootstrapModule` instances
listed in `BootstrapModuleCatalog`. No domain type may be referenced directly
from `Program.cs` or from any non-domain composition module.

### G-PROGCOMP-04 — No Inline Middleware Definition
`Program.cs` MUST NOT define new middleware classes inline or via lambdas
that contain business logic. Middleware composition belongs in
`composition/runtime/RuntimeComposition.cs`.

### G-PROGCOMP-05 — Locked Pipeline Order
The HTTP pipeline order in `Program.cs` MUST remain:
`HttpMetricsMiddleware → UseRouting → UseSwagger → UseSwaggerUI →
MapControllers → MapMetrics → Run`. The locked runtime middleware order
inside `RuntimeComposition` is enforced by `runtime-order.guard.md`.

## Severity
- G-PROGCOMP-01, G-PROGCOMP-03, G-PROGCOMP-04, G-PROGCOMP-05: **S1 — HIGH** (block merge)
- G-PROGCOMP-02: **S2 — MEDIUM** (must resolve in current sprint)
