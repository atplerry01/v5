# TITLE
WBSM v3.5 — Platform Layer Split: API / Host Separation

# CONTEXT
Classification: infrastructure
Context: platform
Domain: host, api

# OBJECTIVE
Enforce strict separation between `/src/platform/api` (external interface layer) and `/src/platform/host` (application bootstrap/DI layer) per new structural rule SDIM-NEW.

# CONSTRAINTS
- API layer references ONLY Shared (contracts)
- Host is the composition root and references all layers for DI wiring
- No controllers or HTTP logic in Host
- No DI container setup in API
- API calls systems layer only (via ISystemIntentDispatcher)
- Build must succeed with zero errors

# EXECUTION STEPS
1. Loaded all 12 guard files for pre-execution validation
2. Created `Whycespace.Api.csproj` — references Shared only, includes FrameworkReference for AspNetCore
3. Moved TodoController and HealthController from host/controllers/ to api/controllers/
4. Moved HealthAggregator from host/health/ to api/health/ (depends only on IHealthCheck from Shared)
5. Updated HealthController namespace from `Whyce.Platform.Host.Controllers` to `Whyce.Platform.Api.Controllers`
6. Updated HealthAggregator namespace from `Whyce.Platform.Host.Health` to `Whyce.Platform.Api.Health`
7. Added Api project reference to Host .csproj
8. Updated Program.cs usings and added `AddApplicationPart()` for controller discovery
9. User refined: moved Swagger config to Api via `SwaggerExtensions.cs`, moved Swashbuckle package to Api .csproj
10. Removed old files from host (controllers/, HealthAggregator.cs)
11. Created `platform.guard.md` to enforce the new boundary
12. Build verified: 0 warnings, 0 errors

# OUTPUT FORMAT
Structured execution report with violations found, folder structure, and refactored file placements.

# VALIDATION CRITERIA
- CHECK-NEW.1: `/src/platform/api` contains ONLY controllers, DTOs, swagger config — PASS
- CHECK-NEW.2: `/src/platform/host` contains ONLY DI, runtime wiring, middleware, health check impls — PASS
- CHECK-NEW.3: No controller or HTTP logic in `/src/platform/host` — PASS
- CHECK-NEW.4: No DI container setup in `/src/platform/api` — PASS
- CHECK-NEW.5: `/src/platform/api` does NOT reference runtime or engines — PASS
- CHECK-NEW.6: `/src/platform/api` calls systems layer ONLY (via ISystemIntentDispatcher) — PASS
- CHECK-NEW.7: `/src/platform/host` wires runtime, infrastructure, and systems together — PASS
- Build: 0 errors, 0 warnings — PASS
