#!/usr/bin/env bash
# Kafka utility — topic consumption and inspection helpers
set -euo pipefail

KAFKA_CONTAINER="${KAFKA_CONTAINER:-whyce-kafka}"
KAFKA_BOOTSTRAP="${KAFKA_BOOTSTRAP:-localhost:9092}"
TODO_EVENTS_TOPIC="${TODO_EVENTS_TOPIC:-whyce.operational.sandbox.todo.events}"

# Consume messages from a topic with timeout, searching for a key/value
# Usage: kafka_topic_contains "$topic" "$search_string" [timeout_ms]
# Returns: 0 if found, 1 if not
kafka_topic_contains() {
    local topic="$1"
    local search="$2"
    local timeout="${3:-10000}"

    docker exec "$KAFKA_CONTAINER" sh -c \
        "/opt/kafka/bin/kafka-console-consumer.sh --bootstrap-server $KAFKA_BOOTSTRAP --topic $topic --from-beginning --timeout-ms $timeout 2>/dev/null" \
        | grep -q "$search"
}

# Count messages on a topic
# Usage: kafka_topic_count "$topic" [timeout_ms]
kafka_topic_count() {
    local topic="$1"
    local timeout="${2:-10000}"

    docker exec "$KAFKA_CONTAINER" sh -c \
        "/opt/kafka/bin/kafka-console-consumer.sh --bootstrap-server $KAFKA_BOOTSTRAP --topic $topic --from-beginning --timeout-ms $timeout 2>/dev/null" \
        | wc -l
}

# Get latest offsets for a topic
kafka_topic_offsets() {
    local topic="$1"
    docker exec "$KAFKA_CONTAINER" sh -c \
        "/opt/kafka/bin/kafka-get-offsets.sh --bootstrap-server $KAFKA_BOOTSTRAP --topic $topic" \
        2>/dev/null
}

# Check if a specific aggregate's event exists on the todo events topic
kafka_has_todo_event() {
    local aggregate_id="$1"
    local timeout="${2:-15000}"
    kafka_topic_contains "$TODO_EVENTS_TOPIC" "$aggregate_id" "$timeout"
}

# Get current consumer group lag
kafka_consumer_lag() {
    local group="${1:-whyce.projection.operational.sandbox.todo}"
    docker exec "$KAFKA_CONTAINER" sh -c \
        "/opt/kafka/bin/kafka-consumer-groups.sh --bootstrap-server $KAFKA_BOOTSTRAP --group $group --describe" \
        2>/dev/null
}
