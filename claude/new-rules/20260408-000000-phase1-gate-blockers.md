# Phase 1 Gate Blockers — Captured Drift

**Source:** Phase 1 Final Gate execution, 2026-04-08
**Evidence:** `claude/audits/phase1-evidence/VERDICT.md`

---

## DRIFT-1 — Missing Kafka topic crashes outbox publisher (S0)

**CLASSIFICATION:** infrastructure / event-fabric
**SOURCE:** `host.log`, `KafkaOutboxPublisher.cs:94`, `outbox` table inspection
**DESCRIPTION:** The outbox holds rows whose `topic` column is
`whyce.constitutional-system.policy.decision.events`, but
`infrastructure/event-fabric/kafka/create-topics.sh` does not provision that topic
family. The publisher hits the poison row and throws
`Confluent.Kafka.ProduceException: Broker: Unknown topic or partition`.
**PROPOSED RULE:** Every event type ever written to `outbox.topic` must have a
corresponding topic in `create-topics.sh`. Add a startup-time guard that diffs
the set of distinct `topic` values in `outbox` against the broker topic list and
fails fast.
**SEVERITY:** S0

---

## DRIFT-2 — KafkaOutboxPublisher has no per-row error isolation (S0)

**CLASSIFICATION:** runtime / outbox
**SOURCE:** `KafkaOutboxPublisher.cs:37,94,104` + `host.log`
**DESCRIPTION:** A single failing row causes the entire
`PublishBatchAsync` recursion to throw, which propagates out of `ExecuteAsync`,
which (with `HostOptions.BackgroundServiceExceptionBehavior=StopHost`) is
supposed to stop the host. Healthy rows queued behind the poison row never
publish. The publisher must isolate per-row failures.
**PROPOSED RULE:** Outbox publishers must catch produce exceptions per-row,
mark the row `status='failed'` with an error message column (or move it to a
dead-letter table), and continue. Never let one bad row halt the publish loop.
**SEVERITY:** S0

---

## DRIFT-3 — `events` table lacks audit envelope columns (S1)

**CLASSIFICATION:** runtime / event-store
**SOURCE:** `\d public.events` schema dump
**DESCRIPTION:** The `events` table has only `id, aggregate_id, aggregate_type,
event_type, payload, version, created_at`. There is no `execution_hash`,
`correlation_id`, `causation_id`, or `policy_decision_hash` column. The Phase 1
Gate explicitly requires the event store to expose `ExecutionHash` per event,
and downstream replay/audit tools cannot trace events back to their policy
decision without these columns. The values exist in the API response and in
`outbox.payload` for *workflow* events, but they are not persisted alongside
domain events.
**PROPOSED RULE:** All persisted domain events must carry the audit envelope
fields (`execution_hash`, `correlation_id`, `causation_id`, `policy_decision_hash`,
`policy_version`) as first-class columns on the `events` table.
**SEVERITY:** S1

---

## DRIFT-4 — `TodoController.Create` bypasses the dispatcher (S1)

**CLASSIFICATION:** platform / api
**SOURCE:** `src/platform/api/controllers/TodoController.cs:35-43` vs `:91-103`
**DESCRIPTION:** `Create` calls `_intentHandler.HandleAsync(intent)` directly and
returns `{status, todoId}`, while `Update` and `Complete` call
`_dispatcher.DispatchAsync(cmd, TodoRoute)` and return the full
`CommandResult` including `auditEmission` (PolicyEvaluatedEvent envelope,
DecisionHash, ExecutionHash, correlation/causation IDs). Two parallel paths
into the runtime is a structural drift; CREATE responses cannot be audited the
same way as UPDATE/COMPLETE responses.
**PROPOSED RULE:** All controller actions must enter the runtime through
`ISystemIntentDispatcher.DispatchAsync` and return `CommandResult`. Direct
`IIntentHandler` calls from controllers are forbidden.
**SEVERITY:** S1

---

## DRIFT-5 — Chain correlation IDs don't match API-returned envelope (S2)

**CLASSIFICATION:** runtime / chain
**SOURCE:** `whyce_chain` rows vs API response correlation IDs
**DESCRIPTION:** Of the 3 commands in this run, only the COMPLETE command's
`correlationId` (`2b8344ec-...`) appears in `whyce_chain.correlation_id`.
UPDATE's correlationId (`3200940b-...`) is not present in the chain at all.
CREATE returns no correlationId at all (see DRIFT-4). This means clients
cannot reliably look up the chain block for a command using the correlation
ID returned in the API response.
**PROPOSED RULE:** The `correlation_id` written to `whyce_chain` must equal the
`correlationId` returned in the API `auditEmission` for that command. No layer
between the dispatcher and the chain anchor may rewrite correlation IDs.
**SEVERITY:** S2

---

## DRIFT-6 — Topic naming drift in Phase 1 Gate prompt (S3)

**CLASSIFICATION:** docs / kafka
**SOURCE:** Phase 1 Gate prompt vs `kafka-topics.sh --list`
**DESCRIPTION:** Prompt references topic
`whyce.operational-system.sandbox.todo.events`, actual topic is
`whyce.operational.sandbox.todo.events` (no `-system` suffix).
**PROPOSED RULE:** Update the Phase 1 Gate prompt to use the canonical topic
name. Add a guard that any topic name written in `claude/` docs must match an
existing topic in `create-topics.sh`.
**SEVERITY:** S3

---

## DRIFT-7 — Destructive command in `start-up.md` (S3)

**CLASSIFICATION:** docs / startup
**SOURCE:** `docs/start-up.md` line 3
**DESCRIPTION:** First documented startup command is
`docker rm -f $(docker ps -aq)`, which deletes ALL docker containers on the
host (including unrelated user containers). This is over-aggressive cleanup.
**PROPOSED RULE:** Replace with `docker compose -f infrastructure/deployment/docker-compose.yml down`
which only removes containers in this compose project.
**SEVERITY:** S3

---

## DRIFT-8 — HSID migration not auto-applied (S3)

**CLASSIFICATION:** infrastructure / postgres
**SOURCE:** `docker-compose.yml` line 66 + host crash log on first run
**DESCRIPTION:** The compose mount only includes
`event-store/migrations:/docker-entrypoint-initdb.d`. The HSID migration
lives at `infrastructure/data/postgres/hsid/migrations/001_hsid_sequences.sql`
and is never auto-applied. The host crashes on startup with `HSID FATAL`
until an operator manually applies it.
**PROPOSED RULE:** Either move the HSID migration into the event-store
migrations directory, or add a second mount that includes the HSID migrations
in the postgres init path.
**SEVERITY:** S3
