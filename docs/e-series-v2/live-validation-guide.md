# LIVE VALIDATION GUIDE -- END-TO-END TESTING WITH POSTMAN

## VERSION: 2.0 (Post-Kanban Validated)

This guide covers everything needed to bring up the full infrastructure,
run E2E validation via Postman (or curl), and verify every infrastructure
store. Written from hard-won lessons during Kanban live validation.

---

# PART 1 -- INFRASTRUCTURE CLI REFERENCE

## 1.1 Environment File

Location: `infrastructure/deployment/.env`

```
POSTGRES_USER=whyce
POSTGRES_PASSWORD=change_me_securely
MINIO_ROOT_USER=whyce
MINIO_ROOT_PASSWORD=change_me_securely
```

All compose services read from this file. Never change POSTGRES_USER
after first volume creation -- Postgres bakes auth into the data volume
on first boot.

## 1.2 Bring Up Everything (Fresh Start)

```bash
cd infrastructure/deployment

# Full clean start (destroys all data volumes)
docker compose -f docker-compose.yml -f multi-instance.compose.yml down -v
docker compose -f docker-compose.yml -f multi-instance.compose.yml up -d
```

This starts 19 containers:

| Container | Purpose | Port |
|-----------|---------|------|
| whyce-host-1 | API instance 1 | 18081 |
| whyce-host-2 | API instance 2 | 18082 |
| whyce-edge | Nginx load balancer (round-robin) | 18080 |
| whyce-postgres | Event Store DB | 5432 |
| whyce-postgres-projections | Projections DB | 5434 |
| whyce-whycechain-db | WhyceChain Ledger DB | 5433 |
| whyce-kafka | Kafka broker (KRaft) | 9092/29092 |
| whyce-redis | Distributed lock + cache | 6379 |
| whyce-opa | Policy engine | 8181 |
| whyce-minio | Object storage | 9000/9001 |
| whyce-prometheus | Metrics scraper | 9090 |
| whyce-grafana | Dashboards | 3000 |
| whyce-kafka-ui | Kafka topic browser | 8080 |
| whyce-pgadmin | Postgres admin UI | 5050 |
| whyce-redisinsight | Redis admin UI | 5540 |
| whyce-kafka-exporter | Kafka metrics for Prometheus | 9308 |
| whyce-postgres-exporter | Postgres metrics for Prometheus | 9187 |
| whyce-redis-exporter | Redis metrics for Prometheus | 9121 |
| whyce-node-exporter | Host metrics for Prometheus | 9100 |

## 1.3 Rebuild After Code Changes

```bash
cd infrastructure/deployment

# Rebuild host images (uses Docker cache, fast)
docker compose -f docker-compose.yml -f multi-instance.compose.yml \
  build whyce-host-1 whyce-host-2

# Redeploy hosts only (infra stays running)
docker compose -f docker-compose.yml -f multi-instance.compose.yml \
  up -d --no-deps whyce-host-1 whyce-host-2
```

IMPORTANT: Use `--no-deps` to avoid recreating infrastructure containers.
Without it, Postgres volumes may be recreated, losing all data and
breaking authentication.

## 1.4 Rebuild Without Cache (When Docker Cache Is Stale)

```bash
docker compose -f docker-compose.yml -f multi-instance.compose.yml \
  build --no-cache whyce-host-1

docker compose -f docker-compose.yml -f multi-instance.compose.yml \
  up -d --no-deps whyce-host-1 whyce-host-2
```

## 1.5 View Logs

```bash
# Recent logs from host-1
docker logs --tail 50 whyce-host-1

# Follow logs live
docker logs -f whyce-host-1

# Check if both Kafka consumers started (CRITICAL)
docker logs whyce-host-1 2>&1 | grep "consumer config"
# Should show TWO lines (one per domain: todo + kanban)
```

## 1.6 Check Container Health

```bash
docker ps --format "table {{.Names}}\t{{.Status}}"
```

All containers should show `(healthy)` or `Up`. The `postgres-extra-migrations`
and `kafka-init` containers are one-shot -- they exit after completing.

## 1.7 Restart Hosts Only

```bash
docker restart whyce-host-1 whyce-host-2
```

Wait 15 seconds for startup, then verify health.

---

# PART 2 -- RESOURCE URLS

## 2.1 API Endpoints

| URL | Purpose |
|-----|---------|
| `http://localhost:18080` | Edge proxy (load balanced, use for testing) |
| `http://localhost:18081` | Direct host-1 access |
| `http://localhost:18082` | Direct host-2 access |

### Health Check

```
GET http://localhost:18080/health
```

Returns:
```json
{
  "status": "HEALTHY",
  "services": [
    {"name": "postgres", "status": "HEALTHY"},
    {"name": "kafka", "status": "HEALTHY"},
    {"name": "redis", "status": "HEALTHY"},
    {"name": "opa", "status": "HEALTHY"},
    {"name": "minio", "status": "HEALTHY"},
    {"name": "runtime", "status": "HEALTHY"},
    {"name": "workers", "status": "HEALTHY"}
  ]
}
```

### Liveness / Readiness

```
GET http://localhost:18080/live     # Process-only liveness
GET http://localhost:18080/ready    # Dependency-aware readiness
```

### Kanban API

| Method | URL | Body |
|--------|-----|------|
| POST | `/api/kanban/board/create` | `{"name":"...","userId":"..."}` |
| POST | `/api/kanban/list/create` | `{"boardId":"...","name":"...","position":0}` |
| POST | `/api/kanban/card/create` | `{"boardId":"...","listId":"...","title":"...","description":"...","position":0}` |
| POST | `/api/kanban/card/move` | `{"boardId":"...","cardId":"...","fromListId":"...","toListId":"...","newPosition":0}` |
| POST | `/api/kanban/card/reorder` | `{"boardId":"...","cardId":"...","listId":"...","newPosition":0}` |
| POST | `/api/kanban/card/complete` | `{"boardId":"...","cardId":"..."}` |
| POST | `/api/kanban/card/update` | `{"boardId":"...","cardId":"...","title":"...","description":"..."}` |
| GET | `/api/kanban/{boardId}` | -- |

### Todo API

| Method | URL | Body |
|--------|-----|------|
| POST | `/api/todo/create` | `{"title":"...","userId":"..."}` |
| POST | `/api/todo/update` | `{"id":"...","title":"..."}` |
| POST | `/api/todo/complete` | `{"id":"..."}` |
| GET | `/api/todo/{id}` | -- |

## 2.2 Admin UIs

| URL | Tool | Credentials |
|-----|------|-------------|
| `http://localhost:8080` | Kafka UI | None (open) |
| `http://localhost:5050` | pgAdmin | admin@whycespace.dev / admin |
| `http://localhost:5540` | RedisInsight | None (open) |
| `http://localhost:3000` | Grafana | admin / admin |
| `http://localhost:9090` | Prometheus | None (open) |
| `http://localhost:9001` | MinIO Console | whyce / change_me_securely |

## 2.3 Internal Service Endpoints (From Within Docker Network)

These are used by the host containers internally:

| Service | Internal URL |
|---------|-------------|
| Postgres Event Store | `postgres:5432` / `whyce_eventstore` |
| Postgres Projections | `postgres-projections:5432` / `whyce_projections` |
| WhyceChain DB | `whycechain-db:5432` / `whycechain` |
| Kafka | `kafka:9092` |
| Redis | `redis:6379` |
| OPA | `http://opa:8181` |
| MinIO | `minio:9000` |

---

# PART 3 -- DATABASE MANAGEMENT

## 3.1 Connecting to Postgres (via docker exec)

```bash
# Event Store
docker exec -it whyce-postgres psql -U whyce -d whyce_eventstore

# Projections
docker exec -it whyce-postgres-projections psql -U whyce -d whyce_projections

# WhyceChain
docker exec -it whyce-whycechain-db psql -U whyce -d whycechain
```

## 3.2 Key Tables

### Event Store (whyce_eventstore)

```sql
-- Domain events
SELECT id, aggregate_id, event_type, version, correlation_id
FROM events WHERE aggregate_id = '{id}' ORDER BY version;

-- Outbox
SELECT id, aggregate_id, event_type, status, retry_count, topic
FROM outbox WHERE aggregate_id = '{id}' ORDER BY created_at;

-- Idempotency
SELECT key, created_at FROM idempotency_keys;
```

### WhyceChain (whycechain)

```sql
SELECT block_id, correlation_id, event_hash,
       LEFT(previous_block_hash, 20) as prev_hash
FROM whyce_chain ORDER BY timestamp DESC LIMIT 10;
```

### Projections (whyce_projections)

```sql
-- Kanban read model
SELECT aggregate_id, current_version, last_event_type, state
FROM projection_operational_sandbox_kanban.kanban_read_model
WHERE aggregate_id = '{boardId}';

-- Todo read model
SELECT aggregate_id, current_version, last_event_type, state
FROM projection_operational_sandbox_todo.todo_read_model
WHERE aggregate_id = '{todoId}';
```

## 3.3 Clean Test Data (Reset for Re-testing)

```bash
# Event store
docker exec whyce-postgres psql -U whyce -d whyce_eventstore \
  -c "DELETE FROM outbox; DELETE FROM events; DELETE FROM idempotency_keys;"

# WhyceChain
docker exec whyce-whycechain-db psql -U whyce -d whycechain \
  -c "DELETE FROM whyce_chain;"

# Projections
docker exec whyce-postgres-projections psql -U whyce -d whyce_projections \
  -c "DELETE FROM projection_operational_sandbox_kanban.kanban_read_model;"
```

## 3.4 Apply Projection Schema for New Domain

```bash
docker exec whyce-postgres-projections psql -U whyce -d whyce_projections \
  -c "$(cat infrastructure/data/postgres/projections/{classification}/{context}/{domain}/001_projection.sql)"
```

## 3.5 pgAdmin Connection Setup

Open `http://localhost:5050`. Add servers:

| Server Name | Host | Port | Database | User | Password |
|-------------|------|------|----------|------|----------|
| Event Store | whyce-postgres | 5432 | whyce_eventstore | whyce | change_me_securely |
| Projections | whyce-postgres-projections | 5434 | whyce_projections | whyce | change_me_securely |
| WhyceChain | whyce-whycechain-db | 5433 | whycechain | whyce | change_me_securely |

NOTE: When connecting from pgAdmin (running inside Docker), use the container
name as hostname (e.g., `whyce-postgres`), NOT `localhost`.

---

# PART 4 -- KAFKA MANAGEMENT

## 4.1 List Topics

```bash
docker exec whyce-kafka sh -c \
  '/opt/kafka/bin/kafka-topics.sh --list --bootstrap-server localhost:9092'
```

## 4.2 Create Topics for New Domain

```bash
DOMAIN="whyce.operational.sandbox.kanban"

docker exec whyce-kafka sh -c \
  "/opt/kafka/bin/kafka-topics.sh --create --bootstrap-server localhost:9092 \
   --topic ${DOMAIN}.events --partitions 3 --replication-factor 1"

docker exec whyce-kafka sh -c \
  "/opt/kafka/bin/kafka-topics.sh --create --bootstrap-server localhost:9092 \
   --topic ${DOMAIN}.deadletter --partitions 1 --replication-factor 1"

docker exec whyce-kafka sh -c \
  "/opt/kafka/bin/kafka-topics.sh --create --bootstrap-server localhost:9092 \
   --topic ${DOMAIN}.retry --partitions 1 --replication-factor 1"
```

IMPORTANT: On Windows with Git Bash, paths get mangled. Always use
`docker exec <container> sh -c '...'` instead of passing paths directly.

## 4.3 Consume Messages (Debug)

```bash
docker exec whyce-kafka sh -c \
  '/opt/kafka/bin/kafka-console-consumer.sh \
   --bootstrap-server localhost:9092 \
   --topic whyce.operational.sandbox.kanban.events \
   --from-beginning --max-messages 5'
```

## 4.4 Check Consumer Group Lag

```bash
docker exec whyce-kafka sh -c \
  '/opt/kafka/bin/kafka-consumer-groups.sh \
   --bootstrap-server localhost:9092 \
   --describe --group whyce.projection.operational.sandbox.kanban'
```

## 4.5 Check Dead Letter Queue

```bash
docker exec whyce-kafka sh -c \
  '/opt/kafka/bin/kafka-console-consumer.sh \
   --bootstrap-server localhost:9092 \
   --topic whyce.operational.sandbox.kanban.deadletter \
   --from-beginning --max-messages 5 \
   --property print.headers=true'
```

## 4.6 Kafka UI

Open `http://localhost:8080`. Browse topics, view messages, check consumer
groups. No configuration needed.

---

# PART 5 -- OPA POLICY MANAGEMENT

## 5.1 List Loaded Policies

```bash
curl -s http://localhost:8181/v1/policies | python -m json.tool
```

Or in Postman: `GET http://localhost:8181/v1/policies`

## 5.2 Load/Update a Policy

```bash
curl -X PUT http://localhost:8181/v1/policies/policies/domain/operational/sandbox/kanban.rego \
  --data-binary @infrastructure/policy/domain/operational/sandbox/kanban.rego \
  -H "Content-Type: text/plain"
```

Returns `{}` on success.

NOTE: OPA policies auto-load from mounted volumes on container start. Manual
loading is only needed when adding policies to a running OPA instance without
restart.

## 5.3 Test a Policy Decision

```bash
curl -s -X POST http://localhost:8181/v1/data/whyce/policy/operational/sandbox/kanban \
  -H "Content-Type: application/json" \
  -d '{
    "input": {
      "action": "kanban.create",
      "subject": {"role": "user"},
      "resource": {
        "classification": "operational",
        "context": "sandbox",
        "domain": "kanban"
      },
      "policy_id": "test"
    }
  }'
```

Expected: `{"result":{"allow":true}}`

## 5.4 Action Name Convention

The runtime derives action names from command types:

```
CreateKanbanBoardCommand  -> kanban.create
MoveKanbanCardCommand     -> kanban.move
ReorderKanbanCardCommand  -> kanban.reorder
CompleteKanbanCardCommand -> kanban.complete
UpdateKanbanCardCommand   -> kanban.update
```

Rule: strip "Command" suffix, find domain name in remaining string, extract
verb = everything before domain name, lowercase. Result = `{domain}.{verb}`.

---

# PART 6 -- REDIS MANAGEMENT

## 6.1 CLI Access

```bash
docker exec -it whyce-redis redis-cli
```

## 6.2 Check Distributed Locks

```bash
docker exec whyce-redis redis-cli KEYS "lock:*"
```

## 6.3 RedisInsight

Open `http://localhost:5540`. Add database: host=`whyce-redis`, port=`6379`.

---

# PART 7 -- POSTMAN COLLECTION SETUP

## 7.1 Environment Variables

Create a Postman environment with:

| Variable | Value |
|----------|-------|
| `base_url` | `http://localhost:18080` |
| `board_id` | (set after board create) |
| `backlog_id` | (set after list create) |
| `inprogress_id` | (set after list create) |
| `done_id` | (set after list create) |
| `card1_id` | (set after card create) |
| `card2_id` | (set after card create) |
| `card3_id` | (set after card create) |

## 7.2 Postman Test Script (Auto-capture IDs)

Add this to the "Tests" tab of each request:

### Board Create

```javascript
if (pm.response.code === 200) {
    var body = pm.response.json();
    pm.environment.set("board_id", body.boardId);
    pm.environment.set("correlation_id", body.correlationId);
}
```

### List Create (Backlog)

```javascript
if (pm.response.code === 200) {
    var body = pm.response.json();
    pm.environment.set("backlog_id", body.listId);
}
```

### List Create (InProgress)

```javascript
if (pm.response.code === 200) {
    pm.environment.set("inprogress_id", pm.response.json().listId);
}
```

### List Create (Done)

```javascript
if (pm.response.code === 200) {
    pm.environment.set("done_id", pm.response.json().listId);
}
```

### Card Creates

```javascript
if (pm.response.code === 200) {
    pm.environment.set("card1_id", pm.response.json().cardId);
}
```

## 7.3 Full Postman Request Sequence

Execute in this exact order. Wait 1-2 seconds between requests for event
processing.

### Request 1: Health Check

```
GET {{base_url}}/health
```

### Request 2: Create Board

```
POST {{base_url}}/api/kanban/board/create
Content-Type: application/json

{
  "name": "Postman Test Board",
  "userId": "postman-user"
}
```

### Request 3: Create Backlog List

```
POST {{base_url}}/api/kanban/list/create
Content-Type: application/json

{
  "boardId": "{{board_id}}",
  "name": "Backlog",
  "position": 0
}
```

### Request 4: Create InProgress List

```
POST {{base_url}}/api/kanban/list/create
Content-Type: application/json

{
  "boardId": "{{board_id}}",
  "name": "InProgress",
  "position": 1
}
```

### Request 5: Create Done List

```
POST {{base_url}}/api/kanban/list/create
Content-Type: application/json

{
  "boardId": "{{board_id}}",
  "name": "Done",
  "position": 2
}
```

### Request 6: Create Card Alpha

```
POST {{base_url}}/api/kanban/card/create
Content-Type: application/json

{
  "boardId": "{{board_id}}",
  "listId": "{{backlog_id}}",
  "title": "Card Alpha",
  "description": "First test card",
  "position": 0
}
```

### Request 7: Create Card Beta

```
POST {{base_url}}/api/kanban/card/create
Content-Type: application/json

{
  "boardId": "{{board_id}}",
  "listId": "{{backlog_id}}",
  "title": "Card Beta",
  "description": "Second test card",
  "position": 1
}
```

### Request 8: Create Card Gamma

```
POST {{base_url}}/api/kanban/card/create
Content-Type: application/json

{
  "boardId": "{{board_id}}",
  "listId": "{{backlog_id}}",
  "title": "Card Gamma",
  "description": "Third test card",
  "position": 2
}
```

### Request 9: Move Alpha to InProgress

```
POST {{base_url}}/api/kanban/card/move
Content-Type: application/json

{
  "boardId": "{{board_id}}",
  "cardId": "{{card1_id}}",
  "fromListId": "{{backlog_id}}",
  "toListId": "{{inprogress_id}}",
  "newPosition": 0
}
```

### Request 10: Reorder Gamma to Position 0

```
POST {{base_url}}/api/kanban/card/reorder
Content-Type: application/json

{
  "boardId": "{{board_id}}",
  "cardId": "{{card3_id}}",
  "listId": "{{backlog_id}}",
  "newPosition": 0
}
```

### Request 11: Complete Alpha

```
POST {{base_url}}/api/kanban/card/complete
Content-Type: application/json

{
  "boardId": "{{board_id}}",
  "cardId": "{{card1_id}}"
}
```

### Request 12: Update Beta

```
POST {{base_url}}/api/kanban/card/update
Content-Type: application/json

{
  "boardId": "{{board_id}}",
  "cardId": "{{card2_id}}",
  "title": "Card Beta UPDATED",
  "description": "Modified via Postman"
}
```

### Request 13: GET Board (wait 10s for projection)

```
GET {{base_url}}/api/kanban/{{board_id}}
```

Expected response: Full board with 3 lists, cards in correct positions,
Alpha completed, Beta updated.

### Request 14: Idempotency Test (repeat board create)

```
POST {{base_url}}/api/kanban/board/create
Content-Type: application/json

{
  "name": "Postman Test Board",
  "userId": "postman-user"
}
```

Expected: `{"error": "Duplicate command detected."}`

---

# PART 8 -- VERIFICATION QUERIES (RUN AFTER POSTMAN SEQUENCE)

Run these after completing the Postman sequence to verify all stores.

```bash
BOARD_ID="<paste board_id from Postman>"

echo "=== EVENT STORE ==="
docker exec whyce-postgres psql -U whyce -d whyce_eventstore -c \
  "SELECT event_type, version FROM events
   WHERE aggregate_id = '$BOARD_ID' ORDER BY version;"

echo ""
echo "=== OUTBOX ==="
docker exec whyce-postgres psql -U whyce -d whyce_eventstore -c \
  "SELECT event_type, status FROM outbox
   WHERE aggregate_id = '$BOARD_ID' ORDER BY created_at;"

echo ""
echo "=== WHYCECHAIN ==="
docker exec whyce-whycechain-db psql -U whyce -d whycechain -c \
  "SELECT COUNT(*) as total_blocks FROM whyce_chain;"

echo ""
echo "=== PROJECTION ==="
docker exec whyce-postgres-projections psql -U whyce -d whyce_projections -c \
  "SELECT current_version, last_event_type
   FROM projection_operational_sandbox_kanban.kanban_read_model
   WHERE aggregate_id = '$BOARD_ID';"
```

### Expected Results

| Store | Expected |
|-------|----------|
| Event Store | 11 events (versions 0-10) |
| Outbox | 11 rows, all status = published |
| WhyceChain | 22+ blocks (11 domain + 11 audit) |
| Projection | current_version > 0, last_event_type = KanbanCardUpdatedEvent |

---

# PART 9 -- IMPORTANT NOTES AND COMMON ISSUES

## 9.1 Windows-Specific Issues

**Git Bash path mangling**: Git Bash on Windows converts Unix paths to
Windows paths in docker exec commands. Always wrap commands in
`sh -c '...'` to prevent this:

```bash
# WRONG (Git Bash mangles /opt/kafka)
docker exec whyce-kafka /opt/kafka/bin/kafka-topics.sh --list

# CORRECT
docker exec whyce-kafka sh -c '/opt/kafka/bin/kafka-topics.sh --list --bootstrap-server localhost:9092'
```

**Line endings**: Shell scripts mounted into Linux containers must have
LF line endings, not CRLF. If a migration script fails with
`$'\r': command not found`, fix with:

```bash
sed -i 's/\r$//' infrastructure/deployment/multi-instance/apply-extra-migrations.sh
```

## 9.2 Volume Authentication Mismatch

If you change POSTGRES_PASSWORD in .env after volumes already exist,
Postgres rejects connections because the password is baked into the
volume on first init. Fix: destroy volumes and recreate.

```bash
docker compose -f docker-compose.yml -f multi-instance.compose.yml down -v
docker compose -f docker-compose.yml -f multi-instance.compose.yml up -d
```

## 9.3 Kafka Consumer Not Starting

Symptom: Only one "Kafka consumer config applied" line in host logs.

Cause: `AddHostedService<T>` deduplicates by implementation type. Two
domains using `GenericKafkaProjectionConsumerWorker` results in only one
consumer starting.

Fix: Use `AddSingleton<IHostedService>` in the second domain's bootstrap.

Verify: Check host logs for BOTH consumer config lines:
```
Kafka consumer config applied for whyce.operational.sandbox.todo.events
Kafka consumer config applied for whyce.operational.sandbox.kanban.events
```

## 9.4 Projection Not Updating

Possible causes (in order of likelihood):

1. **Kafka topic doesn't exist**: Consumer crashes. Check host logs for
   `ConsumeException`. Fix: create topic.
2. **Consumer not running**: Check for "consumer config applied" in logs.
3. **Deserialization failure**: Check logs for `JsonException`. Usually caused
   by value objects in schema records (needs FlexibleIntConverter).
4. **Outbox stuck**: Check `SELECT status, retry_count FROM outbox`. If
   status=failed with high retry_count, the topic didn't exist when publishing
   was attempted. Reset: `UPDATE outbox SET status='pending', retry_count=0, next_retry_at=NULL`.
5. **Projection schema missing**: Check if the migration SQL was applied.

## 9.5 OPA Policy Denied

Symptom: `"error":"OPA policy denied: Policy denied by OPA. No bypass allowed."`

Cause: No rego policy loaded for the domain.

Fix:
```bash
curl -X PUT http://localhost:8181/v1/policies/policies/domain/{c}/{ctx}/{domain}.rego \
  --data-binary @infrastructure/policy/domain/{c}/{ctx}/{domain}.rego \
  -H "Content-Type: text/plain"
```

Verify: `curl http://localhost:8181/v1/policies` should list the policy.

## 9.6 Concurrency Conflict (409)

Symptom: `"type":"urn:whyce:error:concurrency-conflict"`

This is EXPECTED when multiple requests modify the same aggregate
simultaneously. The optimistic concurrency check detects that the aggregate
version changed between load and persist.

Response to caller: retry the request. The second attempt will load the
updated version and succeed.

## 9.7 Duplicate Command Detected

Symptom: `{"error":"Duplicate command detected."}`

This means the exact same command (same CommandId) was already processed.
The idempotency middleware prevents re-execution.

If you need to re-run the same logical operation, change an input parameter
(e.g., different title) so the deterministic CommandId changes. Or clear
the idempotency table:

```bash
docker exec whyce-postgres psql -U whyce -d whyce_eventstore \
  -c "DELETE FROM idempotency_keys;"
```

## 9.8 Full System Reset Sequence

When you need a completely clean slate:

```bash
# 1. Clean all data (keep containers running)
docker exec whyce-postgres psql -U whyce -d whyce_eventstore \
  -c "DELETE FROM outbox; DELETE FROM events; DELETE FROM idempotency_keys;"
docker exec whyce-whycechain-db psql -U whyce -d whycechain \
  -c "DELETE FROM whyce_chain;"
docker exec whyce-postgres-projections psql -U whyce -d whyce_projections \
  -c "DELETE FROM projection_operational_sandbox_kanban.kanban_read_model;"

# 2. Restart hosts (clears in-memory state, reconnects consumers)
docker restart whyce-host-1 whyce-host-2

# 3. Wait and verify
sleep 15
curl -s http://localhost:18080/health
```

## 9.9 Monitoring Outbox Drain

After sending requests, the outbox may take a few seconds to publish
all messages to Kafka. Monitor with:

```bash
# Watch outbox drain in real-time
watch -n 2 "docker exec whyce-postgres psql -U whyce -d whyce_eventstore -t \
  -c \"SELECT status, COUNT(*) FROM outbox GROUP BY status;\""
```

Expected: all rows transition from `pending` to `published`.

## 9.10 Kafka Topic Auto-Creation

Kafka is configured with `KAFKA_AUTO_CREATE_TOPICS_ENABLE: "false"`.
Topics MUST be created explicitly before use. The `kafka-init` container
runs create-topics.sh on first boot, but any domain added after initial
setup needs manual topic creation (see Part 4.2).
