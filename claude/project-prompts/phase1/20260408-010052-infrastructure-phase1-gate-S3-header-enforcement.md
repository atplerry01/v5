# TITLE
phase1-gate-S3 — Consumer header enforcement via deadletter routing

# CONTEXT
Phase 1 Hardening, Tasks 1 + 5 (combined). The projection consumer was
silently committing offsets when event-id/aggregate-id/event-type headers
were missing or unparseable. Spec mandates deadletter-or-fail-fast.
Default chosen earlier: deadletter (fail-fast reproduces the S0 publisher
crash we already fixed).

Classification: infrastructure / phase1-hardening / kafka-consumer
Domain: platform/host adapters

# OBJECTIVE
Replace silent commit-skip with deadletter publish + diagnostic headers.
Consumer never crashes. Offset is committed only after deadletter publish
attempt completes.

# CONSTRAINTS
- $5: no architecture changes; same worker class.
- Topics already provisioned by create-topics.sh as `*.deadletter`.
- Deadletter publish failure must not crash the consumer ($12 spirit).
- Preserve original message key/value/headers in DLQ payload.

# EXECUTION STEPS
1. Build a producer for the deadletter topic inside ExecuteAsync.
2. Derive deadletter topic name: `*.events` → `*.deadletter`.
3. New helper PublishToDeadletterAsync: forwards original message + adds
   `dlq-reason` and `dlq-source-topic` headers; catches all exceptions.
4. Replace each silent commit-skip branch with: deadletter publish → commit.

# OUTPUT FORMAT
Single-file edit to GenericKafkaProjectionConsumerWorker.cs.

# VALIDATION CRITERIA
- dotnet build src/platform/host succeeds.
- Zero remaining `consumer.Commit(result); continue;` paths that don't first
  invoke PublishToDeadletterAsync (other than the happy path).
- Deadletter producer disposed via `using`.
