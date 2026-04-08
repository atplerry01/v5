# NEW RULES — 2026-04-08 14:50 (live e2e attempt findings)

## Finding 5 — Config key dot-vs-double-underscore (CRITICAL S0)
- **CLASSIFICATION:** infrastructure / config-safety
- **SOURCE:** Live host startup attempt during e2e validation, 2026-04-08
- **DESCRIPTION:** 15 callsites in `src/platform/host/composition/**` use `configuration.GetValue<string>("Foo__Bar")` with literal double underscore. .NET `AddEnvironmentVariables()` rewrites env-var `Foo__Bar` to config key `Foo:Bar`. The literal `Foo__Bar` lookup therefore never resolves env vars and the host throws `InvalidOperationException` on every required key. This blocks all live execution.
- **AFFECTED:**
  - `InfrastructureComposition.cs:31,33,36,38,40,96`
  - `ObservabilityComposition.cs:22,24,26,28,48,50,52,54`
  - `TodoBootstrap.cs:44,56,59`
- **PROPOSED_RULE:** Add `CFG-K1` to `claude/guards/config-safety.guard.md`: "Configuration key lookups MUST use the `Section:Key` form, never `Section__Key`. The double-underscore form is the env-var encoding, not the lookup key. CI grep: `GetValue<.*>\(\"[A-Za-z]+__` = S0 fail."
- **SEVERITY:** S0
- **REGRESSION-CONTEXT:** Commit `18a0a75` ("phase1.5-S1: enforce no-fallback connection-string composition") added the throw without exercising the env-var path. Commit `1fcb302` ("externalize OutboxPublisher MAX_RETRY") is unreachable for the same reason — `Outbox__MaxRetry` lookup will silently return null and use the hardcoded fallback.

## Finding 6 — Composition root layer purity (S1)
- **CLASSIFICATION:** structural / dependency-graph
- **SOURCE:** `scripts/dependency-check.sh` run 2026-04-08, exit 1, 40+ violations
- **DESCRIPTION:** `src/platform/host/` is classified `layer=platform` by `dependency-check.sh`, but as the composition root it must reference Runtime, Engines, Projections, Domain. The current rules treat every such reference as a violation, producing constant noise that masks real leaks.
- **PROPOSED_RULE:** Either (a) introduce a `composition` layer classification distinct from `platform` and move `host/` into it, or (b) whitelist `src/platform/host/composition/**` and `src/platform/host/Whycespace.Host.csproj` in `dependency-check.sh`. Document the exemption in `claude/guards/dependency-graph.guard.md`. Adapter files (`host/adapters/*.cs`) are NOT exempted — those should move to runtime or have a justification.
- **SEVERITY:** S1
- **DECISION-OWNER:** architecture lead

## Finding 7 — Observability stack drift (S2)
- **CLASSIFICATION:** infrastructure / observability
- **SOURCE:** `scripts/infrastructure-check.sh` run 2026-04-08, 5/5 fail
- **DESCRIPTION:** Script probes Prometheus + Grafana endpoints; neither container is in `infrastructure/deployment/docker-compose.yml`. Either the compose file is incomplete or the check is stale.
- **PROPOSED_RULE:** Reconcile within one of: add Prometheus + Grafana to compose, or remove probes from infrastructure-check.sh, or split into `infrastructure-check-core.sh` (mandatory) and `infrastructure-check-observability.sh` (optional).
- **SEVERITY:** S2

## Finding 9 — Policy decision hash not propagated to events table (S2/MEDIUM)
- **CLASSIFICATION:** policy / projection
- **SOURCE:** Live e2e test 2026-04-08, todo.create event id `d51e8a7f-06f5-7b40-4c9c-d7797e55f076`
- **DESCRIPTION:** `events.policy_decision_hash` and `events.policy_version` are NULL on the persisted TodoCreatedEvent, even though the WhyceChain block was anchored with a non-empty `decision_hash` for the same correlation_id. Either the dispatcher pipeline does not write the policy hash back to the event row, or the columns are populated by a separate path that didn't run for this slice. Per WBSM v3 $11 (audit trail) and `policy.guard.md`, every state change MUST carry the decision hash on the event itself, not only on the chain.
- **AFFECTED:** `events` table writes from the runtime dispatcher / engine event-emit path
- **PROPOSED_RULE:** Add `P-EVT-001` to `policy.guard.md`: "Persisted events MUST have non-null policy_decision_hash and policy_version. Audit query: `SELECT count(*) FROM events WHERE policy_decision_hash IS NULL` must equal 0 in any environment that has executed at least one command."
- **SEVERITY:** S2 (visibility gap, not data loss — chain still has the hash)

## Finding 10 — Outbox migrations not applied by host startup (S1)
- **CLASSIFICATION:** infrastructure / migrations
- **SOURCE:** Live e2e test 2026-04-08; outbox table was at migration 003 but code expects 005
- **DESCRIPTION:** Host has no migration runner. The outbox schema in `whyce_eventstore` was missing migrations `004_outbox_event_aggregate_ids.sql` and `005_outbox_next_retry_at.sql`, causing a 500 with `column "event_id" of relation "outbox" does not exist` on first command. Operator had to apply manually via `docker exec psql`. There is no in-process migration step in `Program.cs` and no documented bring-up script.
- **PROPOSED_RULE:** Add `INFRA-M1` to `infrastructure.guard.md`: "Host startup MUST verify schema version against expected migration count or run pending migrations. Document the migration runner in `docs/start-up.md`. Manual `docker exec psql` is not acceptable for phase 1.5."
- **SEVERITY:** S1

## Finding 11 — Chain table location ambiguity (S2)
- **CLASSIFICATION:** infrastructure / data-topology
- **SOURCE:** Live e2e test 2026-04-08
- **DESCRIPTION:** `whyce_chain` table physically lives in the `whyce_eventstore` database (port 5432), not the `whycechain` database (port 5433) which is empty. The migration file at `infrastructure/data/postgres/chain/migrations/001_whyce_chain.sql` suggests the chain DB is the intended home, but no migration runner has applied it there. `InfrastructureComposition.cs:33` falls back from `Postgres:ChainConnectionString` to `postgresEventStoreCs`, which is what makes the current setup work — but the fallback masks a topology mismatch.
- **PROPOSED_RULE:** Decide and document: either (a) the chain lives in eventstore DB and the chain DB + migration file should be deleted, or (b) the chain DB is the canonical home and migration runner must apply 001 there before host startup. Capture decision in `docs/infrastructure/`.
- **SEVERITY:** S2

## Finding 8 — Validation harness must run static checks first (S2)
- **CLASSIFICATION:** validation / process
- **SOURCE:** This e2e attempt
- **DESCRIPTION:** Existing `scripts/{deterministic-id,dependency,infrastructure,hsid-infra}-check.sh` provide cheap, executable signal. The original e2e prompt did not invoke them, even though they run in seconds and would have caught CRITICAL-1 and CRITICAL-2 before any live attempt.
- **PROPOSED_RULE:** `scripts/validation/run-e2e.sh` MUST invoke all `scripts/*-check.sh` as STAGE 0 and abort if any fails. Update `e2e-validation.guard.md` G-E2E-011.
- **SEVERITY:** S2
