# TITLE
phase1-gate-S4 — Publisher: emit to *.deadletter Kafka topic on retry exhaustion

# CONTEXT
Phase 1 Hardening, Task 3. RecordFailureAsync already promotes failed
outbox rows to status='deadletter' in the DB after retry exhaustion, but
nothing publishes to the corresponding *.deadletter Kafka topic. Operators
and downstream consumers need real-time visibility.

Classification: infrastructure / phase1-hardening / kafka-deadletter
Domain: platform/host adapters

# OBJECTIVE
On the attempt that exhausts the retry budget, publish the original
payload to the row's `{topic}.deadletter` topic with diagnostic headers
(dlq-reason, dlq-attempts, dlq-source-topic). DB remains source of truth.

# CONSTRAINTS
- $5: no architecture changes; reuses existing _producer.
- DLQ publish failure must NEVER crash publisher loop ($12 spirit).
- Only publish on the final attempt — earlier failures stay status='failed'
  with no DLQ noise.

# EXECUTION STEPS
1. New helper TryPublishToDeadletterAsync(entry, reason, ct).
2. Computes deadletter topic by replacing trailing `.events` → `.deadletter`.
3. Publishes original payload with full header set + dlq-* metadata.
4. Wraps in try/catch — log on failure, do not throw.
5. Called from both ProduceException and generic Exception branches.

# OUTPUT FORMAT
Single-file edit to KafkaOutboxPublisher.cs.

# VALIDATION CRITERIA
- dotnet build succeeds.
- DLQ helper only fires on the budget-exhausting attempt.
- No new exceptions can escape the batch loop.
