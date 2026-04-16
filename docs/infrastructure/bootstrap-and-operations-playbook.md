# Whycespace Infrastructure Bootstrap & Operations Playbook

> **Canonical Operational Playbook — WBSM v3.5**
> Status: LOCKED v1.0
> Scope: Local dev, staging, production-readiness baseline
> Audience: Engineers bootstrapping, validating, operating, and extending Whycespace infrastructure.
>
> **This playbook describes the infrastructure as it actually exists in this repository.** It does not propose new layouts. Any divergence between this document and the contents of [infrastructure/](../../infrastructure/) and [scripts/](../../scripts/) is a bug — fix the doc, not the layout.

---

## 1. Introduction

### 1.1 Purpose

Single source of truth for how to start, validate, operate, and extend the Whycespace infrastructure layer. Defines deterministic procedures, scripts, configuration shape, and failure handling for every primitive used by the runtime, engines, and projections.

### 1.2 Scope

| Environment | Status | Config |
|---|---|---|
| Local | ✅ | [infrastructure/environments/local/environment.json](../../infrastructure/environments/local/environment.json) |
| Dev | ✅ | [infrastructure/environments/dev/environment.json](../../infrastructure/environments/dev/environment.json) |
| Staging | ✅ | [infrastructure/environments/staging/environment.json](../../infrastructure/environments/staging/environment.json) |
| Production | ✅ (baseline) | [infrastructure/environments/production/environment.json](../../infrastructure/environments/production/environment.json) |

### 1.3 Principles

1. **Deterministic infrastructure** — same inputs, same outputs, every time.
2. **Reproducibility** — any environment rebuildable from `git clone` + bootstrap script.
3. **Zero hidden configuration** — every value lives in `environment.json` or compose. Nothing hand-set in containers.
4. **Script-first operations** — humans never `docker exec` to fix things.
5. **Layer purity** — infrastructure lives **outside** `/src`. Runtime *consumes* it; never *defines* it. No C# under [infrastructure/](../../infrastructure/).
6. **Replay safety** — every primitive supports deterministic replay without loss or duplication.

---

## 2. Infrastructure Components Overview

### 2.1 Postgres — Event Store

- **Container:** `whyce-postgres`, image `postgres:15`, port `5432`
- **Database:** `whyce_eventstore`
- **Purpose:** Append-only event store + outbox + chain anchor.
- **Migrations:** [infrastructure/data/postgres/event-store/migrations/](../../infrastructure/data/postgres/event-store/migrations/), [.../outbox/migrations/](../../infrastructure/data/postgres/outbox/migrations/), [.../chain/migrations/](../../infrastructure/data/postgres/chain/migrations/)
- **Used by:** runtime (append, outbox), audit subsystem (chain).

### 2.2 Postgres — Projections

- **Container:** `whyce-postgres-projections`, image `postgres:15`, port `5434`
- **Database:** `whyce_projections`
- **Purpose:** CQRS read models, segregated per classification.
- **Migrations:** [infrastructure/data/postgres/projections/{economic,identity,operational}/](../../infrastructure/data/postgres/projections/)
- **Used by:** projections layer.

### 2.3 Postgres — WhyceChain Ledger

- **Container:** `whyce-whycechain-db`, image `postgres:15`, port `5433`
- **Database:** `whycechain`
- **Purpose:** Chain-of-custody ledger storage backing the WhyceChain anchor service.

### 2.4 Kafka (KRaft mode)

- **Container:** `whyce-kafka`, image `apache/kafka:3.9.0`
- **Ports:** `9092` (internal `kafka:9092`), `29092` (external `localhost:29092`), `9093` (controller)
- **Cluster ID:** `whyce-kafka-kraft-local`
- **Defaults:** 12 partitions, replication-factor 1 (local), `auto.create.topics.enable=false`, lz4 compression.
- **Topic creation:** [infrastructure/event-fabric/kafka/create-topics.sh](../../infrastructure/event-fabric/kafka/create-topics.sh) (run by `kafka-init` sidecar).
- **Used by:** runtime (publish via outbox relay), engines (consume), projections (consume).

### 2.5 Redis

- **Container:** `whyce-redis`, image `redis:7`, port `6379`
- **Purpose:** Hot read-model cache, ephemeral projection state, distributed locks.
- **Persistence:** Volume `redis_data`. Treat as ephemeral — recovery is "flush and rebuild from Postgres."

### 2.6 OPA — Policy Engine

- **Container:** `whyce-opa`, image `openpolicyagent/opa:latest`, port `8181`
- **Policies mounted:** [infrastructure/policy/base/](../../infrastructure/policy/base/), [infrastructure/policy/domain/](../../infrastructure/policy/domain/)
- **Purpose:** Hosts WHYCEPOLICY rules. **All** runtime data-plane operations consult OPA.

### 2.7 MinIO — Object Storage

- **Container:** `whyce-minio`, image `minio/minio`, ports `9000` (S3) / `9001` (console)
- **Init sidecar:** `whyce-minio-init` runs [infrastructure/data/minio/init-buckets.sh](../../infrastructure/data/minio/init-buckets.sh).
- **Purpose:** Large payload + snapshot storage. Metadata always lives in Postgres.

### 2.8 Observability Stack

| Service | Container | Port | Purpose |
|---|---|---|---|
| Prometheus | `whyce-prometheus` | 9090 | Metrics scrape |
| Grafana | `whyce-grafana` | 3000 | Dashboards (anonymous viewer enabled locally) |
| kafka-exporter | `whyce-kafka-exporter` | 9308 | Kafka metrics |
| postgres-exporter | `whyce-postgres-exporter` | 9187 | Postgres metrics |
| redis-exporter | `whyce-redis-exporter` | 9121 | Redis metrics |
| node-exporter | `whyce-node-exporter` | 9100 | Host metrics |

### 2.9 UI Services

| Service | Container | Port | Purpose |
|---|---|---|---|
| kafka-ui | `whyce-kafka-ui` | 8080 | Topic / consumer-group browser |
| pgAdmin | `whyce-pgadmin` | 5050 | Postgres admin (admin@whycespace.dev / admin) |
| RedisInsight | `whyce-redisinsight` | 5540 | Redis browser |

---

## 3. Directory Structure (Canonical — as it exists)

```
infrastructure/
  data/
    postgres/
      event-store/migrations/        # Event store DDL
      outbox/migrations/             # Outbox DDL
      chain/migrations/              # Chain anchor DDL
      projections/                   # Per-classification projection DDL
        economic/
        identity/
        operational/
      hsid/                          # Hierarchical-Stable-ID infra
      system/                        # System tables
    redis/
    minio/
      init-buckets.sh                # MinIO bucket init
  deployment/
    docker-compose.yml               # The (single, canonical) compose file
    scripts/
      bootstrap.sh                   # Full stack up + init
      teardown.sh                    # Full stack down
      migrate.sh                     # Apply event-store migrations
      migrate-projections.sh         # Apply projection migrations
  environments/
    local/environment.json           # Per-env configuration (no .env files)
    dev/environment.json
    staging/environment.json
    production/environment.json
  event-fabric/
    kafka/
      create-topics.sh               # Idempotent topic creation
      topics/                        # Topic manifests by classification
        economic/
        identity/
        operational/
  observability/
    prometheus/prometheus.yml
    grafana/provisioning/
  policy/
    base/                            # OPA base policies
    domain/                          # OPA domain policies

scripts/
  migrations/                        # Projection-side numbered SQL migrations
  dependency-check.sh
  deterministic-id-check.sh
  hsid-infra-check.sh
  infrastructure-check.sh
  outbox-recovery.sql
```

### 3.1 Folder Responsibilities

| Path | Responsibility | Forbidden |
|---|---|---|
| `infrastructure/data/postgres/*/migrations/` | DDL for each Postgres role (event-store, outbox, chain, projections) | Application code |
| `infrastructure/deployment/` | Compose + lifecycle scripts | Per-env values |
| `infrastructure/environments/<env>/environment.json` | All env-specific configuration | Secrets in cleartext (staging/prod use external secret stores) |
| `infrastructure/event-fabric/kafka/` | Topic creation + manifests | Producing/consuming app messages |
| `infrastructure/observability/` | Static Prometheus/Grafana config | Metrics emission code |
| `infrastructure/policy/{base,domain}/` | OPA Rego policies mounted into the OPA container | Runtime policy logic |
| `scripts/` | Repo-wide checks + projection migrations | Service-specific lifecycle (lives in `infrastructure/deployment/scripts/`) |

---

## 4. Docker Compose Setup

The canonical compose file is [infrastructure/deployment/docker-compose.yml](../../infrastructure/deployment/docker-compose.yml). It is **a single file**, not a modular set, and defines the following service groups on the `whyce-network` bridge:

### 4.1 Service Groups

| Group | Services |
|---|---|
| Core data | `kafka`, `postgres`, `postgres-projections`, `redis`, `whycechain-db`, `minio` |
| Policy | `opa` |
| Init sidecars | `kafka-init`, `minio-init` |
| Observability | `prometheus`, `kafka-exporter`, `postgres-exporter`, `redis-exporter`, `node-exporter`, `grafana` |
| UI | `kafka-ui`, `pgadmin`, `redisinsight` |

### 4.2 Volumes (named)

```
kafka_data, postgres_data, projections_data, redis_data,
opa_data, minio_data, whycechain_data,
prometheus_data, pgadmin_data, redisinsight_data, grafana_data
```

### 4.3 Resource Limits (local)

Defined per service via `deploy.resources`. Notable: `kafka` 2 CPU / 2 GB, `postgres` 2 CPU / 1 GB, `postgres-projections` 1 CPU / 512 MB, `whycechain-db` 1 CPU / 512 MB, `redis` 1 CPU / 768 MB, `minio` 1 CPU / 512 MB.

### 4.4 Healthchecks

Every long-lived service ships with a healthcheck. Init sidecars (`kafka-init`, `minio-init`) gate on `service_healthy` of their dependency. Observability and UI services gate on the readiness of the data plane they observe.

### 4.5 Startup Order

Enforced by `bootstrap.sh` in five phases (see §5), **not** by `depends_on` alone — Compose's `depends_on: service_healthy` is used as a secondary guard for sidecars only.

---

## 5. Bootstrap (Step-by-Step)

### 5.1 Prerequisites

- Docker Engine ≥ 24.0
- Docker Compose plugin ≥ 2.20
- Bash ≥ 4 (Git Bash on Windows)
- ≥ 8 GB free RAM, ≥ 20 GB free disk

### 5.2 Sequence

From repo root:

```bash
# 1. Bring up the full stack for an environment (default: local)
bash infrastructure/deployment/scripts/bootstrap.sh           # local
bash infrastructure/deployment/scripts/bootstrap.sh dev       # or dev|staging|production

# 2. Apply event-store migrations
bash infrastructure/deployment/scripts/migrate.sh

# 3. (Projection migrations are applied automatically by bootstrap.sh step [4/5].)

# 4. Verify
docker compose -f infrastructure/deployment/docker-compose.yml ps
```

### 5.3 What `bootstrap.sh` Actually Does

Per [bootstrap.sh](../../infrastructure/deployment/scripts/bootstrap.sh):

1. **`[1/4] Loading environment`** — resolves `infrastructure/environments/<env>/environment.json`, halts if missing.
2. **`[2/5] Starting core infrastructure`** — `kafka`, `postgres`, `postgres-projections`, `redis`, `opa`, `minio`, `prometheus`.
3. **`[3/5] Running init services`** — `kafka-init` (creates topics), `minio-init` (creates buckets).
4. **`[4/5] Running projection migrations`** — invokes `migrate-projections.sh`.
5. **`[5/5] Starting observability and UI services`** — exporters, `kafka-ui`, `pgadmin`, `redisinsight`, `grafana`, `whycechain-db`.

### 5.4 Failure Handling

| Failure | Behavior |
|---|---|
| Environment file missing | `bootstrap.sh` halts with exit 1 before any container starts |
| Healthcheck fails on a core service | `kafka-init` / `minio-init` block (Compose `service_healthy` gate) |
| `kafka-init` fails to create topics | Topics absent → consumers will fail at startup; re-run create-topics |
| `migrate-projections.sh` fails | Bootstrap halts before observability/UI start |

---

## 6. CLI & Scripting Standard

### 6.1 Rules

1. **Bash-first.** All scripts target Bash. Windows uses Git Bash or WSL2.
2. **Idempotent.** Re-running any script must converge to the same state.
3. **Strict mode.** Every script begins with `set -e` (existing canon) — new scripts should prefer `set -euo pipefail` and `IFS=$'\n\t'`.
4. **No interactive prompts.** Scripts read environment + CLI args only.
5. **Exit codes meaningful.** 0 = success; non-zero = failure with stderr message.

### 6.2 Canonical Scripts (as they exist)

#### `infrastructure/deployment/scripts/`

| Script | Purpose | Inputs | Output |
|---|---|---|---|
| [bootstrap.sh](../../infrastructure/deployment/scripts/bootstrap.sh) | Full stack up + init in deterministic phases | `$1` = environment (default `local`) | Healthy stack, `docker compose ps` |
| [teardown.sh](../../infrastructure/deployment/scripts/teardown.sh) | `docker compose down` (volumes preserved) | none | Containers stopped |
| [migrate.sh](../../infrastructure/deployment/scripts/migrate.sh) | Apply event-store migrations to `whyce-postgres` | none | All `*.sql` in `event-store/migrations/` applied |
| [migrate-projections.sh](../../infrastructure/deployment/scripts/migrate-projections.sh) | Apply projection migrations to `whyce-postgres-projections` | none | Per-classification projection DDL applied |

#### `infrastructure/event-fabric/kafka/`

| Script | Purpose |
|---|---|
| [create-topics.sh](../../infrastructure/event-fabric/kafka/create-topics.sh) | Idempotent creation of all canonical topics; runs inside `kafka-init` sidecar |

#### `scripts/` (repo-wide checks)

| Script | Purpose |
|---|---|
| [dependency-check.sh](../../scripts/dependency-check.sh) | Verifies layer-purity / dependency rules |
| [deterministic-id-check.sh](../../scripts/deterministic-id-check.sh) | Scans for `Guid.NewGuid` and other determinism violations |
| [hsid-infra-check.sh](../../scripts/hsid-infra-check.sh) | HSID infra invariants |
| [infrastructure-check.sh](../../scripts/infrastructure-check.sh) | Cross-cutting infra checks |
| [outbox-recovery.sql](../../scripts/outbox-recovery.sql) | Manual outbox recovery procedure |

### 6.3 Script Header Convention (for new scripts)

```bash
#!/usr/bin/env bash
# -----------------------------------------------------------------------------
# Script:   infrastructure/deployment/scripts/<name>.sh
# Purpose:  <one line>
# Inputs:   <env vars / args>
# Output:   <observable result>
# Failure:  <halt conditions>
# -----------------------------------------------------------------------------
set -euo pipefail
IFS=$'\n\t'
```

---

## 7. Postgres Design (Critical)

Three Postgres instances, three responsibility boundaries.

### 7.1 Event Store (`whyce_eventstore`, port 5432)

Owns the canonical event log, the outbox, and the chain anchor table. DDL lives under [infrastructure/data/postgres/event-store/migrations/](../../infrastructure/data/postgres/event-store/migrations/), [.../outbox/migrations/](../../infrastructure/data/postgres/outbox/migrations/), [.../chain/migrations/](../../infrastructure/data/postgres/chain/migrations/), and [.../hsid/migrations/](../../infrastructure/data/postgres/hsid/migrations/) — these files are the source of truth; consult them before adding columns.

> ⚠ **Known gap (phase 1 gate blocker).** Only `event-store/migrations/` is auto-applied today. The compose `postgres` service mounts `../data/postgres/event-store/migrations:/docker-entrypoint-initdb.d`, and [migrate.sh](../../infrastructure/deployment/scripts/migrate.sh) only iterates that same directory. **Outbox, chain, and HSID migrations are not currently applied by bootstrap.** First-run hosts crash with `HSID FATAL` until the HSID migration is applied by hand. Tracked by CHECK-INFRA-TOPICS-01 + CHECK-INFRA-HSID-INITDB-01 in [canonical/infrastructure.audit.md](../../claude/audits/canonical/infrastructure.audit.md). Until this is fixed, after bootstrap run:
> ```bash
> docker exec -i whyce-postgres psql -U whyce -d whyce_eventstore \
>   < infrastructure/data/postgres/hsid/migrations/001_hsid_sequences.sql
> for f in infrastructure/data/postgres/outbox/migrations/*.sql; do
>   docker exec -i whyce-postgres psql -U whyce -d whyce_eventstore < "$f"
> done
> for f in infrastructure/data/postgres/chain/migrations/*.sql; do
>   docker exec -i whyce-postgres psql -U whyce -d whyce_eventstore < "$f"
> done
> ```

**Required columns on the events table:**

| Column | Type | Notes |
|---|---|---|
| `event_id` | TEXT | Deterministic SHA256-derived ID; UNIQUE |
| `aggregate_id` | TEXT | Part of PK |
| `aggregate_type` | TEXT | |
| `version` | BIGINT | Per-aggregate sequence; PK with `aggregate_id` |
| `event_type` | TEXT | `{Domain}{Action}Event` |
| `payload` | JSONB | |
| `metadata` | JSONB | correlation, causation, policy decision, chain anchor |
| `occurred_at` | TIMESTAMPTZ | Domain time from `IClock` (deterministic) |
| `created_at` | TIMESTAMPTZ | Infra-injected; never used by domain |

**Idempotency:** `UNIQUE (event_id)` + deterministic ID makes re-append a no-op via `ON CONFLICT (event_id) DO NOTHING`.

**Concurrency:** `PRIMARY KEY (aggregate_id, version)` forces optimistic concurrency; concurrent writers race and one fails → caller retries with fresh version.

**Audit traceability:** Per phase 1.6-S1.3 (commit `b68fa8c`), the events table carries audit columns for chain + policy traceability. Consult the migrations for the exact column names rather than guessing.

### 7.2 Outbox

Lives alongside the event store (same database, same transaction as the event append). The outbox relay claims rows with `FOR UPDATE SKIP LOCKED`, publishes to Kafka, then sets `dispatched_at` — this is the **only** path from event store to Kafka.

Manual recovery: [scripts/outbox-recovery.sql](../../scripts/outbox-recovery.sql).

### 7.3 Projections (`whyce_projections`, port 5434)

Each projection lives under its own classification schema:

- [infrastructure/data/postgres/projections/economic/](../../infrastructure/data/postgres/projections/economic/)
- [infrastructure/data/postgres/projections/identity/](../../infrastructure/data/postgres/projections/identity/)
- [infrastructure/data/postgres/projections/operational/](../../infrastructure/data/postgres/projections/operational/)

Every projection table must carry `last_event_id` + `last_version` to enforce idempotent apply.

### 7.4 WhyceChain Ledger (`whycechain`, port 5433)

Separate Postgres instance owned by the WhyceChain anchor service. Stores chain anchors, never event payloads.

---

## 8. Kafka Setup

### 8.1 Topic Naming Standard

```
whyce.<classification>.<context>.<domain>.<kind>
```

Where `<kind>` ∈ `commands | events | retry | deadletter`.

### 8.2 Canonical Topics (from `create-topics.sh`)

Each bounded context gets a quad of topics (commands, events, retry, deadletter):

| Classification > Context > Domain | Topics |
|---|---|
| `economic.ledger.transaction` | commands, events, retry, deadletter |
| `identity.access.identity` | commands, events, retry, deadletter |
| `operational.global.incident` | commands, events, retry, deadletter |
| `operational.sandbox.todo` | commands, events, retry, deadletter |
| `constitutional-system.policy.decision` | commands, events, retry, deadletter |

Manifests grouped by classification under [infrastructure/event-fabric/kafka/topics/](../../infrastructure/event-fabric/kafka/topics/).

### 8.3 Partitioning

| Setting | Local | Staging | Production target |
|---|---|---|---|
| Partitions | 12 | 24 | 64+ (per high-throughput topic) |
| Replication factor | 1 | 3 | 3 |
| `min.insync.replicas` | 1 | 2 | 2 |
| Partition key | `aggregate_id` | `aggregate_id` | `aggregate_id` |

Partition key is **always** `aggregate_id` to guarantee per-aggregate ordering.

### 8.4 Topic Creation

`kafka-init` sidecar runs [create-topics.sh](../../infrastructure/event-fabric/kafka/create-topics.sh) on every bootstrap. Idempotent via `--if-not-exists`. To add a new topic, edit the `TOPICS=(...)` array and re-run the sidecar:

```bash
docker compose -f infrastructure/deployment/docker-compose.yml up -d --force-recreate kafka-init
```

### 8.5 Listeners

| Listener | Address | Use |
|---|---|---|
| `INTERNAL` | `kafka:9092` | Container-to-container |
| `EXTERNAL` | `localhost:29092` | Host tools, runtime running outside compose |
| `CONTROLLER` | `:9093` | KRaft quorum |

**Use `localhost:29092` from host-side tools and from runtime when run outside the compose network.** Inside compose, services use `kafka:9092`.

---

## 9. Redis Usage

### 9.1 Key Naming

```
whyce:<purpose>:<scope>:<id>
```

Examples:

```
whyce:cache:projection:workflow:wf-123
whyce:lock:outbox:dispatcher
whyce:lock:chain:anchor
whyce:idem:command:cmd-abc
```

### 9.2 TTL Rules

| Purpose | TTL | Rationale |
|---|---|---|
| `cache:projection:*` | 300s | Hot read; rebuildable from Postgres |
| `lock:*` | 30s | Auto-release on holder death |
| `idem:command:*` | 86400s | 24h replay protection window |

### 9.3 Distributed Locks

Single-instance Redlock variant:

```
SET whyce:lock:<name> <holder-id> NX PX 30000
```

Release via Lua CAS comparing `<holder-id>` before `DEL`. Runtime owns the lock client; infra only provides Redis.

### 9.4 Persistence

Redis runs with default `redis:7` settings. Treat as ephemeral — recovery is "flush and rebuild from Postgres."

---

## 10. Validation & Health Checks

### 10.1 Per-Service Smoke

#### Postgres event store

```bash
docker exec whyce-postgres psql -U whyce -d whyce_eventstore -c "SELECT 1;"
```

#### Postgres projections

```bash
docker exec whyce-postgres-projections psql -U whyce -d whyce_projections -c "SELECT 1;"
```

#### WhyceChain DB

```bash
docker exec whyce-whycechain-db psql -U whyce -d whycechain -c "SELECT 1;"
```

#### Kafka

```bash
docker exec whyce-kafka /opt/kafka/bin/kafka-topics.sh \
  --bootstrap-server localhost:9092 --list
```

Round-trip:

```bash
docker exec whyce-kafka bash -c '
  echo "hc-$(date +%s)" | /opt/kafka/bin/kafka-console-producer.sh \
    --bootstrap-server localhost:9092 \
    --topic whyce.operational.sandbox.todo.events
'
docker exec whyce-kafka /opt/kafka/bin/kafka-console-consumer.sh \
  --bootstrap-server localhost:9092 \
  --topic whyce.operational.sandbox.todo.events \
  --from-beginning --max-messages 1 --timeout-ms 5000
```

#### Redis

```bash
docker exec whyce-redis redis-cli SET whyce:hc:test ok EX 10
docker exec whyce-redis redis-cli GET whyce:hc:test
```

#### OPA

```bash
curl -s http://localhost:8181/health
```

#### MinIO

```bash
curl -fsS http://localhost:9000/minio/health/live
```

### 10.2 Cross-Cutting Checks

Run before commits / in CI:

```bash
bash scripts/infrastructure-check.sh
bash scripts/dependency-check.sh
bash scripts/deterministic-id-check.sh
bash scripts/hsid-infra-check.sh
```

### 10.3 Full Pipeline (manual)

1. POST a command to runtime API.
2. Verify event row in `whyce.events` (port 5432).
3. Verify outbox row.
4. Verify event published to the relevant Kafka topic.
5. Verify projection row in `whyce_projections` (port 5434).
6. Verify chain anchor row (port 5433).

---

## 11. Failure & Recovery

### 11.1 Reset (preserving volumes)

```bash
bash infrastructure/deployment/scripts/teardown.sh
bash infrastructure/deployment/scripts/bootstrap.sh
```

### 11.2 Full Wipe (DANGER)

```bash
bash infrastructure/deployment/scripts/teardown.sh
docker compose -f infrastructure/deployment/docker-compose.yml down -v
bash infrastructure/deployment/scripts/bootstrap.sh
bash infrastructure/deployment/scripts/migrate.sh
```

`down -v` deletes all named volumes. Use only when intentionally wiping local state.

### 11.3 Replay Events

Replay is the primary recovery mechanism.

1. Stop projection consumers.
2. Truncate target projection tables in `whyce_projections`.
3. Reset projection consumer group offsets to earliest:
   ```bash
   docker exec whyce-kafka /opt/kafka/bin/kafka-consumer-groups.sh \
     --bootstrap-server localhost:9092 \
     --group <projection-group> --reset-offsets --to-earliest \
     --all-topics --execute
   ```
4. Restart projection consumers.

Projection apply is idempotent on `(last_event_id, last_version)`, so step 2 is optional for correctness — only needed to free space.

### 11.4 Outbox Recovery

Use [scripts/outbox-recovery.sql](../../scripts/outbox-recovery.sql) when outbox dispatch is stuck. Read it before running.

### 11.5 Recovery Scenarios

| Failure | Recovery |
|---|---|
| Kafka down | Runtime keeps appending to event store + outbox. Outbox relay backs off. On recovery, pending rows drain in order. |
| Event-store Postgres corruption | Restore from backup + WAL. Re-anchor chain from last verified anchor. |
| Projections Postgres corruption | Wipe + replay from Kafka (procedure 11.3). |
| Partial write (event committed, outbox missed) | Impossible — same transaction. |
| Outbox dispatched but Kafka commit lost | `dispatched_at` set inside the same tx as the Kafka ack handler; consumer-side dedup via `event_id` header. |
| Chain anchor diverged | Compare local `chain_hash` vs. external WhyceChain proof. Halt anchoring on mismatch; manual reconciliation. |

---

## 12. Environment Configuration

### 12.1 Configuration Lives in JSON, Not `.env`

Per-environment config files:

- [local](../../infrastructure/environments/local/environment.json)
- [dev](../../infrastructure/environments/dev/environment.json)
- [staging](../../infrastructure/environments/staging/environment.json)
- [production](../../infrastructure/environments/production/environment.json)

Shape (from `local`):

```json
{
  "environment": "local",
  "classification": "operational-system",
  "context": "deployment",
  "domain": "sandbox",
  "settings": {
    "kafka":   { "bootstrap_servers": "localhost:29092", "partitions": 12, "replication_factor": 1 },
    "postgres": {
      "event_store":  { "host": "localhost", "port": 5432, "database": "whyce_eventstore", "user": "whyce" },
      "projections":  { "host": "localhost", "port": 5434, "database": "whyce_projections", "user": "whyce" },
      "whycechain":   { "host": "localhost", "port": 5433, "database": "whycechain",       "user": "whyce" }
    },
    "redis":         { "host": "localhost", "port": 6379 },
    "opa":           { "host": "localhost", "port": 8181 },
    "minio":         { "endpoint": "localhost:9000", "console": "localhost:9001" },
    "observability": { "prometheus": "localhost:9090", "grafana": "localhost:3000" }
  }
}
```

### 12.2 Rules

1. **All values live in `environment.json`.** Compose still uses inline literals for local convenience; staging/production must override via environment variables sourced from secret stores.
2. **No secrets in repo.** Local credentials are placeholders (`whyce`/`whyce`). Staging/prod credentials come from external secret managers — never committed.
3. **Same shape across environments.** All four files share the same JSON schema.
4. **No environment-specific compose files.** Differences live in `environment.json`, not in YAML branches.

### 12.3 Selecting an Environment

```bash
bash infrastructure/deployment/scripts/bootstrap.sh local
bash infrastructure/deployment/scripts/bootstrap.sh dev
bash infrastructure/deployment/scripts/bootstrap.sh staging
bash infrastructure/deployment/scripts/bootstrap.sh production
```

`bootstrap.sh` halts if the corresponding `environment.json` is missing.

---

## 13. Integration with Runtime

### 13.1 Connection Surface

Runtime (in [src/runtime/](../../src/runtime/)) consumes infrastructure via the values defined in `environment.json` and exposed to the runtime process via environment variables:

| Variable | Value (local) | Used For |
|---|---|---|
| `WHYCE_POSTGRES_EVENTSTORE` | `Host=localhost;Port=5432;Database=whyce_eventstore;Username=whyce;Password=whyce` | Event store + outbox |
| `WHYCE_POSTGRES_PROJECTIONS` | `Host=localhost;Port=5434;Database=whyce_projections;Username=whyce;Password=whyce` | Projection writes |
| `WHYCE_POSTGRES_WHYCECHAIN` | `Host=localhost;Port=5433;Database=whycechain;Username=whyce;Password=whyce` | Chain anchor reads |
| `WHYCE_KAFKA_BOOTSTRAP` | `localhost:29092` | Producer + consumer (host-side) |
| `WHYCE_REDIS_CONN` | `localhost:6379` | Cache + locks |
| `WHYCE_OPA_ENDPOINT` | `http://localhost:8181` | Policy decisions |
| `WHYCE_MINIO_ENDPOINT` | `http://localhost:9000` | Object storage |

When the runtime runs **inside** compose, swap host names: `kafka:9092`, `postgres:5432`, `postgres-projections:5432`, `whycechain-db:5432`, `redis:6379`, `opa:8181`, `minio:9000`.

### 13.2 Boundary Rules

- Runtime **reads** these values at startup, sourced from `environment.json`.
- Runtime **never** runs `docker` commands.
- Runtime **never** creates Kafka topics — that's [create-topics.sh](../../infrastructure/event-fabric/kafka/create-topics.sh).
- Runtime **never** runs DB migrations — that's [migrate.sh](../../infrastructure/deployment/scripts/migrate.sh) / [migrate-projections.sh](../../infrastructure/deployment/scripts/migrate-projections.sh).
- Infrastructure **never** imports or references runtime code. No C# under [infrastructure/](../../infrastructure/).

---

## 14. Common Pitfalls (Real)

1. **Wrong Kafka bootstrap address.** Use `localhost:29092` from the host, `kafka:9092` from inside compose. Mixing them up produces silent connection failures.
2. **Wrong Postgres port.** Event store is `5432`, projections is `5434`, whycechain is `5433`. They are separate databases on separate containers.
3. **`auto.create.topics.enable=true`.** Disabled in canon (`KAFKA_AUTO_CREATE_TOPICS_ENABLE: "false"`). Topics must be created via [create-topics.sh](../../infrastructure/event-fabric/kafka/create-topics.sh).
4. **Hardcoded connection strings in `/src`.** Anything not sourced from `environment.json` is a bug. `dependency-check.sh` and `infrastructure-check.sh` will catch most cases.
5. **Non-deterministic event IDs.** `Guid.NewGuid()` and wall-clock IDs break replay. Caught by [deterministic-id-check.sh](../../scripts/deterministic-id-check.sh).
6. **Migrations from runtime startup.** Migrations are infra-scoped. Runtime must fail-fast if schema is missing, not silently apply it.
7. **Mixing projection state into the event store schema.** Projections live in their own database (`whyce_projections`), never in `whyce_eventstore`.
8. **Trusting Compose `depends_on` alone.** It only waits for container start. `bootstrap.sh` orchestrates the deterministic phase order; do not bypass it.
9. **Editing the running Kafka container by hand.** All topic/config changes go through scripts, not `docker exec`.
10. **Committing offsets before processing.** Always commit *after* the projection write is durable.
11. **Storing secrets in `docker-compose.yml`.** Local values are placeholders; staging/prod must externalize via env vars / secret manager.
12. **Using image tag `latest`** for new services. Pin to a specific minor version (most existing services already do; `opa`, `minio`, `kafka-ui`, `pgadmin`, `redisinsight`, `grafana` currently use `latest` and should be tightened before staging promotion).

---

## 15. Extension Guide

### 15.1 Add a New Infrastructure Service

1. Add the service definition to [infrastructure/deployment/docker-compose.yml](../../infrastructure/deployment/docker-compose.yml) on `whyce-network` with a healthcheck.
2. Add static config under `infrastructure/<service>/` (or `infrastructure/data/<service>/` for state).
3. Add a phase to `bootstrap.sh` that brings it up in the correct order.
4. Add the service's connection settings to all four `environment.json` files.
5. Add a smoke check to §10.1.
6. Document the service in §2 of this playbook.
7. If it needs init, add a sidecar service following the `kafka-init` / `minio-init` pattern.

### 15.2 Add a New Kafka Topic

1. Append to the `TOPICS=(...)` array in [create-topics.sh](../../infrastructure/event-fabric/kafka/create-topics.sh) following the `whyce.<classification>.<context>.<domain>.<kind>` standard.
2. Add a manifest entry under [infrastructure/event-fabric/kafka/topics/<classification>/](../../infrastructure/event-fabric/kafka/topics/).
3. Re-run the `kafka-init` sidecar:
   ```bash
   docker compose -f infrastructure/deployment/docker-compose.yml up -d --force-recreate kafka-init
   ```

### 15.3 Add a New Projection

1. Create DDL under `infrastructure/data/postgres/projections/<classification>/NNN_<name>.sql` with `last_event_id` + `last_version` columns on every table.
2. Run `bash infrastructure/deployment/scripts/migrate-projections.sh`.
3. Add the projection process under [src/projections/](../../src/projections/).
4. Use a dedicated Kafka consumer group named `whyce-proj-<name>`.
5. Verify on first run that rows appear within N seconds of a known event.

### 15.4 Scale Kafka

1. Bump `KAFKA_DEFAULT_REPLICATION_FACTOR` and `KAFKA_OFFSETS_TOPIC_REPLICATION_FACTOR` to 3.
2. Add additional broker services with distinct `KAFKA_NODE_ID` and shared `CLUSTER_ID`.
3. Update `KAFKA_CONTROLLER_QUORUM_VOTERS` to include all controllers.
4. Increase partitions on high-throughput topics via `kafka-topics.sh --alter --partitions` (warning: existing keys may move).
5. Set `min.insync.replicas=2` in producer config.
6. Re-run validation per §10.1.

### 15.5 Shard Postgres Event Store

1. Convert `whyce.events` to `PARTITION BY HASH (aggregate_id)` (start with 16 partitions).
2. Migrate existing data via per-partition `INSERT ... SELECT` during a maintenance window.
3. Add per-partition indexes (PG 11+ inherits automatically).
4. For horizontal sharding across hosts, introduce Citus or pg_partman; route writes by `aggregate_id` hash from the runtime DAL.
5. Update outbox relay to claim per-shard with separate workers.

---

## Enforcement Rules (Non-Negotiable)

1. **No infrastructure logic inside `/src`.** Runtime *consumes* infra; never *defines* it.
2. **No C# in [infrastructure/](../../infrastructure/) or [scripts/](../../scripts/).** Bash, YAML, SQL, JSON, Rego only.
3. **Scripts are the only operational interface.** No `docker exec` by hand to fix things — write a script.
4. **Reproducible from zero.** `git clone` → `bash infrastructure/deployment/scripts/bootstrap.sh` → working stack.
5. **Determinism is sacred.** No `Guid.NewGuid`, no wall-clock time in business logic, no `auto.create.topics`, no unpinned image tags in new services.
6. **WHYCEPOLICY governs all data-plane operations.** OPA exists to *enable* policy enforcement, never to bypass it.

---

*End of Playbook.*
