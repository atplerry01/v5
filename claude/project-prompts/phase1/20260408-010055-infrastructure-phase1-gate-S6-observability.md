# TITLE
phase1-gate-S6 — Observability: meters + structured logging for outbox/consumer

# CONTEXT
Phase 1 Hardening, Task 6. KafkaOutboxPublisher had ILogger but no metrics;
GenericKafkaProjectionConsumerWorker used Console.WriteLine throughout and
had no metrics. Spec calls for logs + metrics across outbox/publish/
consumer/projection.

Classification: infrastructure / phase1-hardening / observability
Domain: platform/host adapters

# OBJECTIVE
Add System.Diagnostics.Metrics meters with counters covering the success
and failure paths of both the publisher and the projection consumer.
Replace Console.WriteLine in consumer with ILogger.

# CONSTRAINTS
- $5: no third-party metric library; built-in System.Diagnostics.Metrics only.
- ILogger optional (constructor accepts null) so existing tests don't break.
- Exporter wiring (OTel/Prometheus) is out of scope; meters must be
  consumable via dotnet-counters today.

# EXECUTION STEPS
1. KafkaOutboxPublisher: static Meter "Whyce.Outbox" with counters
   outbox.published, outbox.failed, outbox.deadlettered, outbox.dlq_published.
   Increment in PublishBatchAsync, RecordFailureAsync, TryPublishToDeadletterAsync.
2. GenericKafkaProjectionConsumerWorker: static Meter "Whyce.Projection.Consumer"
   with counters consumer.consumed, consumer.dlq_routed, consumer.handler_invoked.
   Add ILogger<T>? constructor param; replace all Console.WriteLine with structured log calls.
3. TodoBootstrap: pass ILogger from DI into the worker constructor.

# OUTPUT FORMAT
- 3 file edits: KafkaOutboxPublisher, GenericKafkaProjectionConsumerWorker, TodoBootstrap.

# VALIDATION CRITERIA
- dotnet build succeeds with 0 warnings.
- Zero Console.WriteLine in either adapter.
- Meters can be observed via `dotnet-counters monitor --meters Whyce.Outbox,Whyce.Projection.Consumer`.
