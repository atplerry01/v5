#!/bin/bash
set -e

BOOTSTRAP_SERVER="${KAFKA_BOOTSTRAP_SERVERS:-kafka:9092}"
PARTITIONS=12
REPLICATION=1

echo "Waiting for Kafka to be ready..."
sleep 5

# Domain-aligned topics: whyce.{classification}.{context}.{domain}.{type}
# Each bounded context gets: commands, events, retry, deadletter (dual-topic + DLQ)

TOPICS=(
  # economic > ledger > transaction
  "whyce.economic.ledger.transaction.commands"
  "whyce.economic.ledger.transaction.events"
  "whyce.economic.ledger.transaction.retry"
  "whyce.economic.ledger.transaction.deadletter"

  # identity > access > identity
  "whyce.identity.access.identity.commands"
  "whyce.identity.access.identity.events"
  "whyce.identity.access.identity.retry"
  "whyce.identity.access.identity.deadletter"

  # operational > global > incident
  "whyce.operational.global.incident.commands"
  "whyce.operational.global.incident.events"
  "whyce.operational.global.incident.retry"
  "whyce.operational.global.incident.deadletter"

  # operational > sandbox > todo
  "whyce.operational.sandbox.todo.commands"
  "whyce.operational.sandbox.todo.events"
  "whyce.operational.sandbox.todo.retry"
  "whyce.operational.sandbox.todo.deadletter"

  # operational > sandbox > kanban
  "whyce.operational.sandbox.kanban.commands"
  "whyce.operational.sandbox.kanban.events"
  "whyce.operational.sandbox.kanban.retry"
  "whyce.operational.sandbox.kanban.deadletter"

  # constitutional > policy > decision (Phase 1 gate unblock S0)
  "whyce.constitutional.policy.decision.commands"
  "whyce.constitutional.policy.decision.events"
  "whyce.constitutional.policy.decision.retry"
  "whyce.constitutional.policy.decision.deadletter"
)

for TOPIC in "${TOPICS[@]}"; do
  echo "Creating topic: $TOPIC"
  /opt/kafka/bin/kafka-topics.sh \
    --bootstrap-server "$BOOTSTRAP_SERVER" \
    --create \
    --if-not-exists \
    --topic "$TOPIC" \
    --partitions "$PARTITIONS" \
    --replication-factor "$REPLICATION" \
    2>/dev/null || echo "Topic $TOPIC may already exist"
done

echo "All topics created."
