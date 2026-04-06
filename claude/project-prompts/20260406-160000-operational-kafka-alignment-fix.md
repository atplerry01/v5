---
TITLE: Kafka Topic Routing + Projection Type Alignment + Topic Provisioning
CONTEXT: operational > sandbox > todo — full read/write path fix
OBJECTIVE: Fix 4 blocking defects: Kafka topic routing (dynamic), projection type mismatch, Kafka topic provisioning, outbox-to-Kafka contract
CONSTRAINTS: No hardcoded topics, projections consume domain events only, topics.json matches script, no silent failures
CLASSIFICATION: operational
EXECUTION STEPS:
  1. Create TopicNameResolver for canonical topic resolution from EventEnvelope
  2. Add routing metadata (Classification/Context/Domain) to EventEnvelope and CommandContext
  3. Add DomainRoute record, thread routing through ISystemIntentDispatcher → IWorkflowDispatcher → intent handlers
  4. Add topic column to outbox, update PostgresOutboxAdapter to store topic
  5. Remove hardcoded "whyce.events" from KafkaOutboxPublisher, read topic from outbox row
  6. Normalize TodoProjectionBridge/Handler/Consumer to use domain events (TodoCreatedEvent) instead of schema wrappers (TodoCreatedEventSchema)
  7. Add throw on unmatched events in bridge/consumer
  8. Add todo topics to create-topics.sh (aligned with topics.json)
OUTPUT FORMAT: Code changes + validation
VALIDATION CRITERIA: All source projects build, no hardcoded topics, projections receive domain events, Kafka topics provisioned
---
