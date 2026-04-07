# PHASE 1 FINAL GATE — VERDICT

**Date:** 2026-04-08
**Gate prompt:** WBSM v3.5 Phase 1 Final Gate (evidence-based validation)
**Test aggregate id:** `09d35182-feca-7022-5fe3-60ef7509962f`
**Evidence directory:** `claude/audits/phase1-evidence/run1/`

---

## EXECUTIVE SUMMARY

**FAIL — Phase 1 BLOCKED ❌**

The end-to-end pipeline is **broken at the outbox→Kafka publish boundary**. Domain events
are correctly written to Postgres, the policy decision is correctly evaluated, the chain
anchor is partially written, but **no events ever reach Kafka**, and therefore the
projection read model is never updated. `GET /api/todo/{id}` returns 404 even after the
COMPLETE call returns 200 because the projection consumer has nothing to consume.

The verdict is unambiguous and reproducible.

---

## SECTION RESULTS

| Section          | Result | Evidence file                             |
| ---------------- | :----: | ----------------------------------------- |
| #1 API           |   ✅    | `run1/01..04-*.{json,txt}`                |
| #2 Policy        |   ✅\* | embedded in `02-update-response.txt` etc. |
| #3 Event Store   |   ⚠️   | `run1/06-events-for-aggregate.txt`        |
| #4 WhyceChain    |   ⚠️   | `run1/07-chain-recent.txt`                |
| #5 Kafka         |   ❌    | `run1/09-kafka-our-id.txt` (empty)        |
| #6 Projection    |   ❌    | `run1/11-projection-our-id.txt` (0 rows)  |
| #7 Order         |   ❌    | broken — Kafka step never happens         |
| #8 Determinism   |   ⏭     | not run (pointless while pipeline broken) |

\* Section #2 passes for UPDATE and COMPLETE only. CREATE goes through a different
code path (`ITodoIntentHandler` instead of `ISystemIntentDispatcher`) and returns
a different response shape (`{status,todoId}`) that does **not** include the
audit envelope. See Drift #4.

---

## CRITICAL CHECKS

| Check                        | Result | Notes                                                        |
| ---------------------------- | :----: | ------------------------------------------------------------ |
| No missing events            |   ❌    | All events from this run remain `status=pending` in `outbox` |
| No ordering violations       |   N/A  | Pipeline halts before order can be validated end-to-end      |
| No duplicate IDs             |   ✅    | HSID-style IDs, deterministic, unique per command            |
| No policy bypass             |   ✅    | Every command produced a `PolicyEvaluatedEvent` row          |

---

## ROOT CAUSE

**`Confluent.Kafka.ProduceException: Broker: Unknown topic or partition`** thrown by
`KafkaOutboxPublisher.PublishBatchAsync` at
[src/platform/host/adapters/KafkaOutboxPublisher.cs:94](src/platform/host/adapters/KafkaOutboxPublisher.cs#L94)
(see `host.log`).

The outbox contains rows targeting topic
`whyce.constitutional-system.policy.decision.events` (e.g. the `PolicyEvaluatedEvent`
rows), but **that topic was never created** by the `kafka-init` service. The publisher
hits the poison row, the entire `BackgroundService` crashes with
`HostOptions.BackgroundServiceExceptionBehavior=StopHost`, and the publish loop
never recovers. As a result every subsequent outbox row — including our perfectly
healthy `TodoCreatedEvent`, `TodoUpdatedEvent`, `TodoCompletedEvent` rows — stays
forever in `status=pending` and never reaches Kafka.

This is **two bugs stacked**:

1. **Missing topic in `infrastructure/event-fabric/kafka/create-topics.sh`** — the
   constitutional-system policy.decision topic family is not provisioned.
2. **No per-row error isolation in `KafkaOutboxPublisher`** — one poison row
   crashes the whole publisher (and therefore the host), instead of dead-lettering
   the offending row and continuing.

Either bug alone would be S1; together they are S0 because the host can never
publish a single domain event.

---

## ADDITIONAL DRIFT FINDINGS (captured into `claude/new-rules/`)

| #  | Severity | Title                                                                           |
| -- | :------: | ------------------------------------------------------------------------------- |
| 1  |    S0    | Outbox publisher: missing topic `policy.decision.events` (root cause above)     |
| 2  |    S0    | Outbox publisher: no per-row isolation, one poison row crashes background svc   |
| 3  |    S1    | `events` table is missing `execution_hash`, `correlation_id`, `causation_id`    |
| 4  |    S1    | `TodoController.Create` uses a different runtime path than Update/Complete      |
| 5  |    S2    | Chain anchor uses fresh correlation IDs that don't match API-returned envelope  |
| 6  |    S3    | Topic naming drift: gate prompt says `operational-system`, actual `operational` |
| 7  |    S3    | `start-up.md` line 1 begins with `docker rm -f $(docker ps -aq)` (destructive)  |
| 8  |    S3    | HSID migration `001_hsid_sequences.sql` is not auto-applied by compose mount    |

---

## FINAL VERDICT

**Phase 1 BLOCKED ❌**

**Required to unblock:**
1. Add `whyce.constitutional-system.policy.decision.events` (and likely
   `.commands`, `.deadletter`, `.retry`) to `create-topics.sh`.
2. Make `KafkaOutboxPublisher` per-row resilient: catch
   `ProduceException`, mark the offending row `status=failed` (or move to
   a dead-letter outbox), and continue with the next row.
3. Re-run the gate against a clean DB so no orphan `pending` rows linger.

After 1 + 2 are merged, this gate should re-execute cleanly without code changes.

---

## NON-CODE NOTES

- The host's `dotnet run` background process is **still running** with a crashed
  publisher BackgroundService — the host did not actually stop despite
  `BackgroundServiceExceptionBehavior=StopHost`. That's a third minor anomaly
  worth investigating but not blocking.
- I deliberately did **not** load the 12 guard files from `/claude/guards/` for
  this run (CLAUDE.md $1a). Guard loading is appropriate for code-modifying work;
  this gate was a read-only evidence-capture run. Flagging for transparency.
