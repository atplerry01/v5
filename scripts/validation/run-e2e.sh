#!/usr/bin/env bash
# run-e2e.sh — Whycespace Phase 1.5 end-to-end validation harness
# Classification: validation / phase1.5-gate
#
# Exercises: API -> Runtime -> Engine -> Event Store -> Kafka -> Projection -> Read API
# For each test: emits a §13 block to stdout AND appends to the report file.
#
# Usage:
#   ./run-e2e.sh                    # full run, requires live stack
#   ./run-e2e.sh --dry-run          # parse + emit NOT EXECUTED rows, no network
#   API_BASE=http://localhost:5000 KAFKA_BOOTSTRAP=localhost:9092 ./run-e2e.sh

set -euo pipefail

API_BASE="${API_BASE:-http://localhost:5000}"
KAFKA_BOOTSTRAP="${KAFKA_BOOTSTRAP:-localhost:9092}"
PG_CONN="${PG_CONN:-postgres://whyce:whyce@localhost:5432/whyce}"
REPORT="${REPORT:-docs/validation/e2e-validation-report.md}"
DRY_RUN=0

for arg in "$@"; do
  case "$arg" in
    --dry-run) DRY_RUN=1 ;;
    *) echo "unknown arg: $arg" >&2; exit 2 ;;
  esac
done

log()  { printf '[run-e2e] %s\n' "$*" >&2; }
fail() { printf '[run-e2e] FAIL: %s\n' "$*" >&2; exit 1; }

emit_block() {
  local name="$1" status="$2" notes="${3:-}"
  cat <<EOF
TEST NAME: $name
STATUS: $status

REQUEST:
RESPONSE:

EVENTS:
- event_type:
- event_id:
- aggregate_id:

KAFKA:
- topic:
- partition:

PROJECTION:
- expected:
- actual:

POLICY:
- decision:
- decision_hash:

CHAIN:
- block_id:
- hash:

NOTES: $notes
---
EOF
}

# ---- Section 1+2: TODO domain CRUD lifecycle ----
todo_create()    { emit_block "todo.create"    "FAIL — NOT EXECUTED" "dry-run: stack not contacted"; }
todo_update()    { emit_block "todo.update"    "FAIL — NOT EXECUTED" "dry-run"; }
todo_complete()  { emit_block "todo.complete"  "FAIL — NOT EXECUTED" "dry-run"; }
todo_fetch()     { emit_block "todo.fetch"     "FAIL — NOT EXECUTED" "dry-run"; }

# ---- Section 3: Workflow lifecycle ----
workflow_op_single()   { emit_block "workflow.operational.single-step" "FAIL — NOT EXECUTED" ""; }
workflow_op_multi()    { emit_block "workflow.operational.multi-step"  "FAIL — NOT EXECUTED" ""; }
workflow_lifecycle()   { emit_block "workflow.lifecycle.start-process-complete" "FAIL — NOT EXECUTED" ""; }
workflow_resume()      { emit_block "workflow.lifecycle.resume-after-failure"   "FAIL — NOT EXECUTED" ""; }

# ---- Section 4: Policy ----
policy_allow() { emit_block "policy.allow" "FAIL — NOT EXECUTED" "must capture decision_hash + version"; }
policy_deny()  { emit_block "policy.deny"  "FAIL — NOT EXECUTED" "must verify execution blocked"; }

# ---- Section 5: Chain anchoring ----
chain_anchor()        { emit_block "chain.anchor.deterministic-hash" "FAIL — NOT EXECUTED" ""; }
chain_continuity()    { emit_block "chain.continuity.preserved"      "FAIL — NOT EXECUTED" ""; }

# ---- Section 6: Kafka ----
kafka_topic_naming()  { emit_block "kafka.topic.naming"        "FAIL — NOT EXECUTED" ""; }
kafka_partition_det() { emit_block "kafka.partition.deterministic" "FAIL — NOT EXECUTED" ""; }
kafka_order()         { emit_block "kafka.order.preserved"     "FAIL — NOT EXECUTED" ""; }

# ---- Section 8: Determinism + replay ----
replay_equivalence()  { emit_block "replay.byte-equal-projection" "FAIL — NOT EXECUTED" ""; }

# ---- Section 9: Projection rebuild ----
projection_rebuild()  { emit_block "projection.rebuild-from-scratch" "FAIL — NOT EXECUTED" ""; }

# ---- Section 11: Observability ----
observability_corr()  { emit_block "observability.correlation-id-propagation" "FAIL — NOT EXECUTED" ""; }

# ---- Section 12: Failure recovery ----
recovery_restart()    { emit_block "recovery.service-restart-mid-execution" "FAIL — NOT EXECUTED" ""; }

run_all() {
  todo_create; todo_update; todo_complete; todo_fetch
  workflow_op_single; workflow_op_multi; workflow_lifecycle; workflow_resume
  policy_allow; policy_deny
  chain_anchor; chain_continuity
  kafka_topic_naming; kafka_partition_det; kafka_order
  replay_equivalence
  projection_rebuild
  observability_corr
  recovery_restart
}

stage0_static_checks() {
  log "STAGE 0: static checks (G-E2E-011)"
  local rc=0
  for s in scripts/deterministic-id-check.sh scripts/dependency-check.sh \
           scripts/infrastructure-check.sh scripts/hsid-infra-check.sh; do
    if [[ -x "$s" || -f "$s" ]]; then
      if bash "$s" > "/tmp/$(basename "$s").out" 2>&1; then
        log "  PASS $s"
      else
        log "  FAIL $s (see /tmp/$(basename "$s").out)"
        rc=1
      fi
    fi
  done
  return $rc
}

main() {
  if [[ $DRY_RUN -eq 1 ]]; then
    log "dry-run: emitting NOT EXECUTED block for every test"
    run_all
    exit 0
  fi

  if ! stage0_static_checks; then
    log "STAGE 0 failed — aborting per G-E2E-011"
    exit 1
  fi

  log "live mode: API_BASE=$API_BASE KAFKA=$KAFKA_BOOTSTRAP"
  log "live execution not implemented in this scaffold — operator must wire calls per src/platform endpoints"
  log "exiting non-zero so phase1.5 gate is NOT silently passed"
  run_all
  exit 1
}

main "$@"
