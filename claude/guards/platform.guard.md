# Platform Guard — API / Host Boundary Enforcement

## Purpose

Enforce strict separation between `/src/platform/api` (external interface layer) and `/src/platform/host` (application host/bootstrap layer). The API layer owns controllers, DTOs, and swagger configuration. The host layer owns DI wiring, runtime configuration, middleware registration, and infrastructure adapter instantiation.

## Scope

All files under `src/platform/`. Applies to every file in `api/` and `host/` subdirectories. Evaluated at CI, code review, and architectural audit.

## Rules

### G-PLATFORM-01: API Layer Purity

`/src/platform/api/` MUST contain ONLY:
- Controllers (`[ApiController]`)
- DTOs and request/response types
- Swagger/OpenAPI configuration
- Health aggregation (composition of health check results)

### G-PLATFORM-02: Host Layer Purity

`/src/platform/host/` MUST contain ONLY:
- DI container setup (`builder.Services.*`)
- Runtime wiring and middleware registration
- Infrastructure adapter implementations (health checks, in-memory stores)
- Application bootstrap (`Program.cs`)

### G-PLATFORM-03: No Controllers in Host

BLOCK if any file in `/src/platform/host/` contains:
- `[ApiController]` attribute
- `MapGet`, `MapPost`, `MapPut`, `MapDelete`, `MapPatch` calls that define business endpoints

### G-PLATFORM-04: No DI in API

BLOCK if any file in `/src/platform/api/` contains:
- `IServiceCollection` extensions that register infrastructure adapters
- Direct `builder.Services.*` calls (except swagger/controller registration in extension methods)

### G-PLATFORM-05: API Must Not Reference Runtime or Engines

BLOCK if any file in `/src/platform/api/` references:
- `src/runtime` namespaces (`Whyce.Runtime.*`)
- `src/engines` namespaces (`Whyce.Engines.*`)
- `infrastructure` namespaces

### G-PLATFORM-06: API Calls Systems Layer Only

API controllers MUST dispatch through `ISystemIntentDispatcher` or shared contracts only. No direct engine or runtime type usage in controller code.

### G-PLATFORM-07: Host Wires All Layers

Host (`Program.cs`) is the composition root and MAY reference runtime, engines, systems, domain, and infrastructure for DI registration purposes only.

---

## Check Procedure

1. Scan `/src/platform/api/` for `using` directives referencing `Whyce.Runtime.*`, `Whyce.Engines.*`, or `Whyce.Infrastructure.*`.
2. Scan `/src/platform/host/` for `[ApiController]` attribute declarations.
3. Scan `/src/platform/host/` for `MapGet`, `MapPost`, `MapPut`, `MapDelete` endpoint definitions.
4. Verify `/src/platform/api/*.csproj` references ONLY `Whycespace.Shared` (no Runtime, Engines, Domain, Systems, Projections).
5. Verify `/src/platform/host/*.csproj` references `Whycespace.Api` for controller discovery.
6. Verify all controllers reside in `/src/platform/api/controllers/`.
7. Verify namespace alignment: api files use `Whyce.Platform.Api.*`, host files use `Whyce.Platform.Host.*`.

## Severity Levels

| Severity | Condition | Example |
|----------|-----------|---------|
| **S0 — CRITICAL** | API references engine/runtime directly | `using Whyce.Engines.T2E;` in controller |
| **S0 — CRITICAL** | Controller in host | `[ApiController]` in `/src/platform/host/` |
| **S1 — HIGH** | API registers infrastructure in DI | `services.AddSingleton<IEventStore>()` in api |
| **S1 — HIGH** | Host exposes HTTP endpoints | `app.MapGet("/api/...")` in Program.cs |
| **S2 — MEDIUM** | Namespace misalignment | Api file using `Whyce.Platform.Host.*` namespace |

## Enforcement Action

- **S0**: Block merge. Fail CI. Immediate remediation.
- **S1**: Block merge. Fail CI. Must resolve before merge.
- **S2**: Warn in CI. Must resolve within sprint.

All violations produce a structured report:
```
PLATFORM_GUARD_VIOLATION:
  file: <path>
  rule: <G-PLATFORM-XX>
  severity: <S0-S3>
  violation: <description>
  expected: <correct placement>
  actual: <detected violation>
  remediation: <fix instruction>
```
