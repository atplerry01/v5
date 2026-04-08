# CONFIG SAFETY AUDIT — OUTPUT

**Date:** 2026-04-08
**Sweep ID:** 20260408-132840
**Scope:** src/, infrastructure/, tests/
**Verdict:** **FAIL — BLOCKING** (3 × S0, 5 × S1, 8 × S2, 1 × S3)

This is the sole blocker for the Phase 1.5 gate.

---

## S0 — HARDCODED CREDENTIALS (BLOCKING)

### CFG-S0-01 — Postgres event store credentials in appsettings.json
- **File:** src/platform/host/appsettings.json:2
- **Value:** `"Host=localhost;Port=5432;Database=whyce_eventstore;Username=whyce;Password=whyce"`
- **Fix:** Remove from appsettings.json. InfrastructureComposition.cs:30-31 already enforces no-fallback at startup; this defaults defeats it. Source from `EVENTSTORE__CONNECTIONSTRING` env var only.

### CFG-S0-02 — MinIO access key in appsettings.json
- **File:** src/platform/host/appsettings.json:7
- **Value:** `"MinIO__AccessKey": "whyce"`
- **Fix:** Remove. Source from `MINIO__ACCESSKEY` env var only.

### CFG-S0-03 — MinIO secret key in appsettings.json
- **File:** src/platform/host/appsettings.json:8
- **Value:** `"MinIO__SecretKey": "whycepassword"`
- **Fix:** Remove. Source from `MINIO__SECRETKEY` env var only.

---

## S1 — HARDCODED PRODUCTION ENDPOINTS

### CFG-S1-01 — Kafka bootstrap servers default
- **File:** src/platform/host/appsettings.json:4
- **Value:** `"Kafka__BootstrapServers": "localhost:29092"`
- **Fix:** Remove default; require env var.

### CFG-S1-02 — Redis endpoint default
- **File:** src/platform/host/appsettings.json:3
- **Value:** `"Redis__ConnectionString": "localhost:6379"`
- **Fix:** Remove default; require env var.

### CFG-S1-03 — OPA endpoint default
- **File:** src/platform/host/appsettings.json:5
- **Value:** `"OPA__Endpoint": "http://localhost:8181"`
- **Fix:** Remove default; require env var.

### CFG-S1-04 — MinIO endpoint default
- **File:** src/platform/host/appsettings.json:6
- **Value:** `"MinIO__Endpoint": "localhost:9000"`
- **Fix:** Remove default; require env var.

### CFG-S1-05 — Hardcoded Postgres projections fallback in TodoBootstrap
- **File:** src/platform/host/composition/operational/sandbox/todo/TodoBootstrap.cs:44, 57
- **Value:** `?? "Host=localhost;Port=5434;Database=whyce_projections;Username=whyce;Password=whyce"`
- **Fix:** Remove `??` fallback. Throw `InvalidOperationException` when env var missing (match InfrastructureComposition pattern).

### CFG-S1-06 — Direct configuration indexer + hardcoded fallback in TodoController
- **File:** src/platform/api/controllers/TodoController.cs:31-32
- **Value:** `configuration["Projections__ConnectionString"] ?? "Host=localhost;Port=5434;..."`
- **Fix:** (a) replace dictionary indexer with `configuration.GetValue<string>(...)`; (b) remove fallback; (c) ideally inject `IOptions<ProjectionsOptions>` instead of raw `IConfiguration`.

---

## S2 — MAGIC TIMEOUTS / VALUES

| ID | File | Line | Value | Fix |
|---|---|---|---|---|
| CFG-S2-01 | InfrastructureComposition.cs | 62 | OPA HTTP `Timeout = 5s` | Move to `IOptions<OpaClientOptions>` |
| CFG-S2-02 | ObservabilityComposition.cs | 42 | Health HTTP `Timeout = 5s` | Move to `IOptions<HealthCheckOptions>` |
| CFG-S2-03 | GenericKafkaProjectionConsumerWorker.cs | 177 | Retry delay `5s` | `IOptions<ProjectionConsumerOptions>` |
| CFG-S2-04 | GenericKafkaProjectionConsumerWorker.cs | 182 | Retry delay `1s` | Same as above |
| CFG-S2-05 | KafkaOutboxPublisher.cs | 21 | `DefaultMaxRetryCount = 5` | `IOptions<OutboxOptions>` |
| CFG-S2-06 | KafkaOutboxPublisher.cs | 46 | Poll interval `1s` default | Constructor param exists; acceptable |
| CFG-S2-07 | RedisHealthCheck.cs | 27 | Test-key expiry `5s` | Acceptable in health check |
| CFG-S2-08 | KafkaHealthCheck.cs | 27 | Metadata fetch `5s` | Acceptable in health check |

---

## S3 — DOMAIN-SHAPED CONSTANTS (ALLOWED)

- **CFG-S3-01:** TodoBootstrap.cs:59-63 — `topic = "whyce.operational.sandbox.todo.events"` and projection schema names. ALLOWED — domain-specific schema contracts, not credentials. Externalize only if per-env topic routing is needed.

---

## POSITIVE FINDINGS
- InfrastructureComposition.cs:30-40 — correctly uses `GetValue<T>()` + explicit `throw` no-fallback pattern
- ObservabilityComposition.cs:22-54 — same pattern
- BootstrapModuleCatalog — centralized module registration

## REMEDIATION ORDER (for fix pass)
1. **Wipe credentials & defaults from appsettings.json** (CFG-S0-01..03, CFG-S1-01..04). Replace with empty strings or remove keys entirely.
2. **TodoBootstrap.cs:44,57** — remove `??` fallback, throw on missing env var.
3. **TodoController.cs:31-32** — switch to `GetValue<T>()` + no fallback (or inject IOptions).
4. **S2 magic values** — extract to Options classes (lower priority, can defer).
5. Add a guard test asserting appsettings.json contains no credential-shaped strings.
