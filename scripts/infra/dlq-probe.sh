#!/usr/bin/env bash
# dlq-probe.sh — consume the deadletter topics in the running validation
# stack and record per-topic counts. Any non-zero count is reported; the
# presence of poisoned messages alone is not a failure (replay may be the
# expected response), but a count reconciliation against producer-reported
# deadletters is done by the test suite, not by this probe.
#
# Emits:
#   tests/reports/infra/dlq-probe-<ts>.json
#
# Classification : phase6-hardening / economic-system
# Usage          : ./scripts/infra/dlq-probe.sh [max-messages-per-topic]

set -u

MAX="${1:-100}"

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
OUT_DIR="$REPO_ROOT/tests/reports/infra"
mkdir -p "$OUT_DIR"
OUT_JSON="$OUT_DIR/dlq-probe-$(date -u +'%Y%m%dT%H%M%SZ').json"

# Resolve DLQ topic list from create-topics.sh so the probe stays in sync
# with R-K-17 (every emitting BC declares its topics there).
TOPIC_SCRIPT="$REPO_ROOT/infrastructure/event-fabric/kafka/create-topics.sh"
if [[ ! -f "$TOPIC_SCRIPT" ]]; then
  printf '[dlq-probe] topic script missing: %s\n' "$TOPIC_SCRIPT" >&2
  exit 2
fi

mapfile -t DLQ_TOPICS < <(grep -oE '"whyce\.economic\.[a-z.]+\.deadletter"' "$TOPIC_SCRIPT" | tr -d '"' | sort -u)
if [[ ${#DLQ_TOPICS[@]} -eq 0 ]]; then
  printf '[dlq-probe] no economic-system deadletter topics found in %s\n' "$TOPIC_SCRIPT" >&2
  exit 2
fi

results=()
total=0
for topic in "${DLQ_TOPICS[@]}"; do
  count=$(docker exec whyce-kafka bash -c "
    /opt/kafka/bin/kafka-console-consumer.sh \
      --bootstrap-server localhost:9092 \
      --topic $topic \
      --from-beginning \
      --timeout-ms 3000 \
      --max-messages $MAX 2>/dev/null | wc -l
  " 2>/dev/null || echo 0)
  count="${count//[^0-9]/}"
  count="${count:-0}"
  total=$(( total + count ))
  results+=( "\"$topic\": $count" )
done

joined=$(IFS=,; printf '%s' "${results[*]}")

cat >"$OUT_JSON" <<EOF
{
  "probed_utc": "$(date -u +'%Y-%m-%dT%H:%M:%SZ')",
  "max_messages_per_topic": $MAX,
  "total_messages_on_dlq": $total,
  "per_topic": { $joined }
}
EOF

printf '[dlq-probe] total=%d wrote %s\n' "$total" "$OUT_JSON" >&2
