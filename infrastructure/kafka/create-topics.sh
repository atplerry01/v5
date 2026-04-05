#!/bin/bash
set -e

BOOTSTRAP_SERVER="${KAFKA_BOOTSTRAP_SERVERS:-kafka:9092}"
PARTITIONS=12
REPLICATION=1

echo "Waiting for Kafka to be ready..."
sleep 5

TOPICS=(
  "whyce.economic.ledger.transaction.commands"
  "whyce.economic.ledger.transaction.events"
  "whyce.economic.ledger.transaction.retry"
  "whyce.economic.ledger.transaction.deadletter"
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
