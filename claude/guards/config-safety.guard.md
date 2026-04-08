# CONFIG SAFETY GUARD

**Status:** ACTIVE
**Severity baseline:** S0/S1 violations FAIL the build.
**Owner:** WBSM v3 structural integrity.

## SCOPE
All files under `src/`, `infrastructure/`. Test fixtures under `tests/` are explicitly out of scope unless they hit production-shared files.

## RULES

### CFG-R1 — No credentials in source-controlled config files (S0)
**Forbidden in `appsettings*.json`, `*.config`, any committed config file:**
- Username/Password key-value pairs
- API keys, secret keys, access tokens
- Any string matching `Password=`, `Pwd=`, `SecretKey`, `AccessKey`, `ApiKey`, `Token=`
- Any non-empty value under keys ending in `Password`, `SecretKey`, `AccessKey`

**Exception:** None. Use environment variables exclusively.

### CFG-R2 — No hardcoded production endpoint defaults in source-controlled config (S1)
Forbidden literal values in `appsettings*.json` for keys representing endpoints (`*ConnectionString`, `*BootstrapServers`, `*Endpoint`, `*Url`, `*Host`, `*Port`):
- `localhost`, `127.0.0.1`, `0.0.0.0`
- Any port literal (`5432`, `6379`, `9092`, `29092`, `8181`, `9000`, etc.)
- Any URL literal

Endpoints MUST be sourced from environment variables at startup. Composition root MUST throw `InvalidOperationException` when missing — no `??` fallback.

### CFG-R3 — No hardcoded fallbacks in C# composition code (S1)
Forbidden pattern in any `.cs` file under `src/`:
```csharp
configuration["X"] ?? "Host=...;Password=..."
configuration.GetValue<string>("X") ?? "literal-default"
```
Required pattern (no-fallback):
```csharp
var value = configuration.GetValue<string>("X")
    ?? throw new InvalidOperationException("X is required");
```

### CFG-R4 — Use IOptions<T> over raw IConfiguration in business code (S2)
Controllers, handlers, services MUST inject strongly-typed `IOptions<TOptions>` (or `IOptionsMonitor<T>`). Direct `IConfiguration` indexer access is restricted to:
- `src/platform/host/composition/**` (composition root)
- `Program.cs`

Magic timeouts/intervals/retry counts in business logic must come from an Options class.

### CFG-R5 — appsettings.json shape
The committed `appsettings.json` MUST contain only:
- Logging configuration
- Feature flags (boolean)
- Domain-shaped constants (e.g., topic names that are part of the schema contract)
- Empty placeholder structure for env-bound keys (or omit them entirely)

## CI ENFORCEMENT
1. **Architecture test:** scan `src/platform/host/appsettings*.json` for `Password=`, `SecretKey`, `AccessKey`, `localhost`, port literals — fail on any hit.
2. **Architecture test:** grep `src/**/*.cs` for `?? "Host=` and `?? "Server=` and `configuration\[` outside composition — fail on any hit.
3. **Build:** composition root must be wired to throw on missing env vars (verified by integration test that boots host with empty environment and asserts `InvalidOperationException`).

## ALLOWED EXCEPTIONS
- `src/platform/host/composition/observability/ObservabilityComposition.cs` and `InfrastructureComposition.cs` may directly read `IConfiguration` to bootstrap options.
- Health-check timeouts may be hardcoded (not part of operational SLA).
- Domain-shaped Kafka topic constants (e.g., `whyce.operational.sandbox.todo.events`) are schema contracts, not configuration.

## REMEDIATION TEMPLATE
```csharp
// BEFORE
var cs = configuration["EventStore__ConnectionString"]
    ?? "Host=localhost;Username=whyce;Password=whyce";

// AFTER
var cs = configuration.GetValue<string>("EventStore:ConnectionString")
    ?? throw new InvalidOperationException(
        "EVENTSTORE__CONNECTIONSTRING environment variable is required");
```
