# tests.audit.md

Post-execution audit for the test surface. Pairs with `claude/guards/tests.guard.md`.

## SCOPE
- `tests/integration/**`
- `tests/e2e/**`
- All in-memory test doubles used by either the test projects or production composition.

## CHECKS

### CHECK-T-BUILD-01 — Integration project compiles (S0)
Run `dotnet build tests/integration/Whycespace.Tests.Integration.csproj`.
A failed build is an immediate S0. CI gate MUST fail on this. The historical
regression: `TodoPipelineTests.cs` referenced a stale `RuntimeCommandDispatcher`
constructor signature and was silently red.

### CHECK-T-DOUBLES-01 — Determinism in test doubles (S1)
grep test doubles (`InMemoryChainAnchor`, `InMemoryOutbox`, `InMemoryEventStore`,
any `*Stub`, `*Fake`, `*InMemory*`) for `Guid.NewGuid` / `DateTime*.UtcNow`.
Any hit = S1. Doubles MUST take `IClock` + `IIdGenerator` constructor parameters.

### CHECK-T-PLACEHOLDER-01 — Placeholder repositories (S3)
Every in-memory repository wired into production composition (`Program.cs`,
`*Bootstrap.cs`) MUST be tagged as placeholder AND have a sibling migration in
`scripts/migrations/`. Missing migration = S2; missing tag = S3.

### CHECK-T-DISPATCHER-CTOR-01 — Test instantiation drift (S0)
For each `new RuntimeCommandDispatcher(...)` site under `tests/**`, the argument
list MUST match the current source signature in
`src/runtime/dispatcher/RuntimeCommandDispatcher.cs`. Mismatch = S0.

## SCORING

| Severity | Penalty |
|---|---|
| S0 violation | -40 per occurrence |
| S1 violation | -10 per occurrence |
| S2 violation | -5 per occurrence |
| S3 violation | -1 per occurrence |
| Floor | 0 |
| Pass threshold | >= 80 |
