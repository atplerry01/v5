# Whycespace Infrastructure Quick Reference

> Cheatsheet for day-to-day interaction with the local stack defined in [infrastructure/deployment/docker-compose.yml](../../infrastructure/deployment/docker-compose.yml).
> Bootstrap: `bash infrastructure/deployment/scripts/bootstrap.sh`

---

## 1. Access URLs

### Data Plane

| Service | Host:Port | Internal (compose) | Credentials |
|---|---|---|---|
| Postgres — event store | `localhost:5432` | `postgres:5432` | `whyce` / `whyce` — db `whyce_eventstore` |
| Postgres — projections | `localhost:5434` | `postgres-projections:5432` | `whyce` / `whyce` — db `whyce_projections` |
| Postgres — WhyceChain | `localhost:5433` | `whycechain-db:5432` | `whyce` / `whyce` — db `whycechain` |
| Kafka (host) | `localhost:29092` | — | none |
| Kafka (in-cluster) | — | `kafka:9092` | none |
| Redis | `localhost:6379` | `redis:6379` | none |
| OPA | http://localhost:8181 | `opa:8181` | none |
| MinIO S3 | http://localhost:9000 | `minio:9000` | `whyce` / `whycepassword` |
| MinIO Console | http://localhost:9001 | — | `whyce` / `whycepassword` |

### Observability

| Service | URL | Credentials |
|---|---|---|
| Prometheus | http://localhost:9090 | none |
| Grafana | http://localhost:3000 | `admin` / `admin` (anonymous viewer enabled) |
| kafka-exporter metrics | http://localhost:9308/metrics | none |
| postgres-exporter metrics | http://localhost:9187/metrics | none |
| redis-exporter metrics | http://localhost:9121/metrics | none |
| node-exporter metrics | http://localhost:9100/metrics | none |

### UI / Admin

| Service | URL | Credentials |
|---|---|---|
| Kafka UI | http://localhost:8080 | none |
| pgAdmin | http://localhost:5050 | `admin@whycespace.dev` / `admin` |
| RedisInsight | http://localhost:5540 | none |

---

## 2. Docker CLI

### Lifecycle

```bash
# Bootstrap (default: local; also accepts dev|staging|production)
bash infrastructure/deployment/scripts/bootstrap.sh
bash infrastructure/deployment/scripts/bootstrap.sh dev

# Teardown (preserves volumes)
bash infrastructure/deployment/scripts/teardown.sh

# Full wipe (DANGER — deletes all named volumes)
docker compose -f infrastructure/deployment/docker-compose.yml down -v

# Apply event-store migrations (only)
bash infrastructure/deployment/scripts/migrate.sh

# Apply projection migrations
bash infrastructure/deployment/scripts/migrate-projections.sh
```

### Status & Inspection

```bash
# All services
docker compose -f infrastructure/deployment/docker-compose.yml ps

# Logs (follow)
docker compose -f infrastructure/deployment/docker-compose.yml logs -f kafka
docker logs -f whyce-postgres
docker logs --tail 100 whyce-kafka

# Health
docker inspect -f '{{.State.Health.Status}}' whyce-postgres
docker inspect -f '{{.State.Health.Status}}' whyce-kafka

# Resource usage
docker stats whyce-kafka whyce-postgres whyce-redis
```

### Restart / Recreate Single Service

```bash
docker compose -f infrastructure/deployment/docker-compose.yml restart kafka
docker compose -f infrastructure/deployment/docker-compose.yml up -d --force-recreate kafka-init
```

### Exec Into Containers

```bash
docker exec -it whyce-postgres             psql -U whyce -d whyce_eventstore
docker exec -it whyce-postgres-projections psql -U whyce -d whyce_projections
docker exec -it whyce-whycechain-db        psql -U whyce -d whycechain
docker exec -it whyce-redis                redis-cli
docker exec -it whyce-kafka                bash
```

### Network

```bash
docker network inspect whyce-network
```

### Volume Management

```bash
docker volume ls | grep whyce
docker volume inspect deployment_postgres_data
# Wipe a single volume (stack must be down first)
docker volume rm deployment_postgres_data
```

---

## 3. Kafka CLI

All commands run inside `whyce-kafka` via `docker exec`. Bootstrap server: `localhost:9092` (inside the container).

```bash
# List topics
docker exec whyce-kafka /opt/kafka/bin/kafka-topics.sh \
  --bootstrap-server localhost:9092 --list

# Describe a topic
docker exec whyce-kafka /opt/kafka/bin/kafka-topics.sh \
  --bootstrap-server localhost:9092 --describe \
  --topic whyce.operational.sandbox.todo.events

# Create a topic (idempotent via --if-not-exists)
docker exec whyce-kafka /opt/kafka/bin/kafka-topics.sh \
  --bootstrap-server localhost:9092 --create --if-not-exists \
  --topic whyce.foo.bar.baz.events --partitions 12 --replication-factor 1

# Produce a test message
docker exec -i whyce-kafka /opt/kafka/bin/kafka-console-producer.sh \
  --bootstrap-server localhost:9092 \
  --topic whyce.operational.sandbox.todo.events <<< 'hello'

# Consume from earliest
docker exec whyce-kafka /opt/kafka/bin/kafka-console-consumer.sh \
  --bootstrap-server localhost:9092 \
  --topic whyce.operational.sandbox.todo.events \
  --from-beginning --max-messages 5 --timeout-ms 5000

# Consumer groups
docker exec whyce-kafka /opt/kafka/bin/kafka-consumer-groups.sh \
  --bootstrap-server localhost:9092 --list

docker exec whyce-kafka /opt/kafka/bin/kafka-consumer-groups.sh \
  --bootstrap-server localhost:9092 --describe --group <group>

# Reset offsets to earliest (replay)
docker exec whyce-kafka /opt/kafka/bin/kafka-consumer-groups.sh \
  --bootstrap-server localhost:9092 \
  --group <group> --reset-offsets --to-earliest --all-topics --execute

# Recreate canonical topics from manifest
docker compose -f infrastructure/deployment/docker-compose.yml \
  up -d --force-recreate kafka-init
```

---

## 4. Postgres SQL

### Connect

```bash
docker exec -it whyce-postgres             psql -U whyce -d whyce_eventstore
docker exec -it whyce-postgres-projections psql -U whyce -d whyce_projections
docker exec -it whyce-whycechain-db        psql -U whyce -d whycechain
```

Or from the host:

```bash
psql -h localhost -p 5432 -U whyce -d whyce_eventstore
psql -h localhost -p 5434 -U whyce -d whyce_projections
psql -h localhost -p 5433 -U whyce -d whycechain
```

### Discovery

```sql
-- List schemas
\dn

-- List tables in current schema
\dt
\dt whyce.*
\dt whyce_proj_*.*

-- Describe a table
\d whyce.events

-- Indexes
\di whyce.*

-- Sizes
SELECT relname, pg_size_pretty(pg_total_relation_size(relid))
FROM pg_catalog.pg_statio_user_tables
ORDER BY pg_total_relation_size(relid) DESC
LIMIT 20;
```

### Event Store — Common Queries

```sql
-- Count events
SELECT count(*) FROM whyce.events;

-- Latest 20 events across all aggregates
SELECT event_id, aggregate_id, version, event_type, occurred_at
FROM whyce.events
ORDER BY occurred_at DESC
LIMIT 20;

-- All events for one aggregate, in order
SELECT version, event_type, payload, occurred_at
FROM whyce.events
WHERE aggregate_id = '<id>'
ORDER BY version ASC;

-- Events by type within a window
SELECT count(*) FROM whyce.events
WHERE event_type = 'WorkflowExecutionStartedEvent'
  AND occurred_at >= now() - interval '1 hour';

-- Find events by correlation id (in metadata JSONB)
SELECT event_id, event_type, occurred_at
FROM whyce.events
WHERE metadata->>'correlation_id' = '<corr-id>'
ORDER BY occurred_at;
```

### Outbox — Operational Queries

```sql
-- Pending dispatch backlog
SELECT count(*) FROM whyce.outbox WHERE dispatched_at IS NULL;

-- Oldest pending
SELECT outbox_id, topic, attempts, enqueued_at, last_error
FROM whyce.outbox
WHERE dispatched_at IS NULL
ORDER BY enqueued_at ASC
LIMIT 20;

-- Failure rate
SELECT topic,
       count(*) FILTER (WHERE attempts > 0) AS retried,
       count(*) FILTER (WHERE dispatched_at IS NULL AND attempts >= 5) AS stuck
FROM whyce.outbox
GROUP BY topic;

-- Manual recovery procedure
\i scripts/outbox-recovery.sql
```

### Projections — Common Queries

```sql
-- Per-projection schemas
\dn whyce_proj_*

-- All projection tables
\dt whyce_proj_*.*

-- Lag check (compare last_version against event-store max version)
SELECT workflow_id, last_version, updated_at
FROM whyce_proj_workflow.workflow_state
ORDER BY updated_at DESC
LIMIT 20;
```

### Chain Anchor

```sql
-- Latest anchors
SELECT anchor_id, event_id, prev_anchor_id, anchored_at
FROM whyce.chain_anchor
ORDER BY anchored_at DESC
LIMIT 10;

-- Verify chain continuity (count any breaks where prev_anchor_id doesn't link)
SELECT count(*)
FROM whyce.chain_anchor c
LEFT JOIN whyce.chain_anchor p ON p.anchor_id = c.prev_anchor_id
WHERE c.prev_anchor_id IS NOT NULL AND p.anchor_id IS NULL;
```

### Maintenance

```sql
-- Connections
SELECT pid, usename, application_name, state, query
FROM pg_stat_activity
WHERE datname IN ('whyce_eventstore','whyce_projections','whycechain');

-- Kill an idle connection
SELECT pg_terminate_backend(<pid>);

-- Vacuum / analyze
VACUUM (ANALYZE) whyce.events;

-- Reindex (long-running; offline preferred)
REINDEX TABLE whyce.events;
```

### Manual Migration Application (until phase 1 gate fix lands)

```bash
# HSID (required on first run to avoid HSID FATAL)
docker exec -i whyce-postgres psql -U whyce -d whyce_eventstore \
  < infrastructure/data/postgres/hsid/migrations/001_hsid_sequences.sql

# Outbox
for f in infrastructure/data/postgres/outbox/migrations/*.sql; do
  docker exec -i whyce-postgres psql -U whyce -d whyce_eventstore < "$f"
done

# Chain
for f in infrastructure/data/postgres/chain/migrations/*.sql; do
  docker exec -i whyce-postgres psql -U whyce -d whyce_eventstore < "$f"
done
```

---

## 5. Redis CLI

```bash
docker exec -it whyce-redis redis-cli

# Inside redis-cli:
PING
KEYS whyce:*           # avoid in prod; use SCAN
SCAN 0 MATCH whyce:* COUNT 100
GET whyce:cache:projection:workflow:wf-123
TTL whyce:lock:outbox:dispatcher
DEL whyce:cache:projection:workflow:wf-123
FLUSHDB                # wipe current db (DANGER)
INFO memory
```

---

*See also: [bootstrap-and-operations-playbook.md](bootstrap-and-operations-playbook.md) for the full operational playbook.*
