#!/usr/bin/env bash
# Scenario A — functional success path across 12 economic-system contexts.
# Executes one representative POST per context, captures response bodies,
# eventstore + outbox deltas, and logs each run.
set -uo pipefail

BASE="${BASE:-http://localhost:18081}"
EVIDENCE_DIR="${EVIDENCE_DIR:-$(cd "$(dirname "$0")" && pwd)}"
TOKEN="$(cat /tmp/token.jwt)"
# RUN_ID makes deterministic-HSID-derived aggregate IDs unique per run,
# so idempotency middleware doesn't collapse the happy path into a
# "Duplicate command detected" 400 on re-runs. Exposed for override.
RUN_ID="${RUN_ID:-$(date +%s)}"

psql_events() {
  docker exec whyce-postgres psql -U whyce -d whyce_eventstore -t -c "$1" 2>/dev/null | tr -d ' \n'
}

baseline_events=$(psql_events "SELECT count(*) FROM events;")
baseline_outbox=$(psql_events "SELECT count(*) FROM outbox;")
baseline_published=$(psql_events "SELECT count(*) FROM outbox WHERE status='published';")
printf 'BASELINE: events=%s outbox=%s published=%s\n' \
  "$baseline_events" "$baseline_outbox" "$baseline_published" \
  | tee "$EVIDENCE_DIR/_baseline.txt"

# Stable GUIDs so re-runs are idempotent-ish (aggregate IDs will differ via HSID,
# but the inputs are reproducible).
declare -A REQ
REQ[capital]='POST /api/capital/account/open {"meta":null,"data":{"ownerId":"11111111-1111-1111-1111-111111111111","currency":"USD"}}'
REQ[compliance]='POST /api/compliance/audit/create {"meta":null,"data":{"sourceDomain":"economic.capital.vault","sourceAggregateId":"22222222-2222-2222-2222-222222222222","sourceEventId":"33333333-3333-3333-3333-333333333333","auditType":"Financial","evidenceSummary":"ops-validator scenario A"}}'
REQ[enforcement]='POST /api/enforcement/lock/lock {"meta":null,"data":{"subjectId":"44444444-4444-4444-4444-444444444444","scope":"account","reason":"ops-validator scenario A"}}'
REQ[exchange]='POST /api/exchange/fx/register {"meta":null,"data":{"baseCurrency":"EUR","quoteCurrency":"USD-'$RUN_ID'"}}'
REQ[ledger]='POST /api/economic/ledger/open {"meta":null,"data":{"reference":"ops-validator-scenario-a-'$RUN_ID'","currency":"USD"}}'
REQ[reconciliation]='POST /api/economic/reconciliation/discrepancy/detect {"meta":null,"data":{"processReference":"55555555-5555-5555-5555-555555555555","source":"Projection","expectedValue":100.00,"actualValue":99.50,"difference":0.50}}'
REQ[revenue]='POST /api/economic/contract/create {"meta":null,"data":{"termStart":"2026-04-18T00:00:00Z","termEnd":"2027-04-18T00:00:00Z","shareRules":[]}}'
REQ[risk]='POST /api/risk/exposure/create {"meta":null,"data":{"sourceId":"66666666-6666-6666-6666-666666666666","exposureType":1,"initialExposure":1000.00,"currency":"USD"}}'
REQ[routing]='POST /api/routing/execution/start {"meta":null,"data":{"pathId":"77777777-7777-7777-7777-777777777777"}}'
REQ[subject]='POST /api/economic/subject/register {"meta":null,"data":{"subjectType":"Participant","structuralRefType":"Participant","structuralRefId":"88888888-8888-8888-8888-888888888888","economicRefType":"CapitalAccount","economicRefId":"99999999-9999-9999-9999-999999999999"}}'
REQ[transaction]='POST /api/charge/calculate {"meta":null,"data":{"transactionId":"aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa","type":"Fixed","baseAmount":100.00,"chargeAmount":2.50,"currency":"USD"}}'
REQ[vault]='POST /api/economic/vault/account/create {"meta":null,"data":{"ownerSubjectId":"bbbbbbbb-bbbb-bbbb-bbbb-'$(printf %012d $(($RUN_ID % 1000000000000)))'","currency":"USD"}}'

run_one() {
  local ctx="$1"
  local line="${REQ[$ctx]}"
  local method="${line%% *}"; line="${line#* }"
  local path="${line%% *}"; local body="${line#* }"

  local outfile="$EVIDENCE_DIR/${ctx}-response.json"
  local metafile="$EVIDENCE_DIR/${ctx}-meta.txt"
  local url="$BASE$path"

  local code
  code=$(curl -s -o "$outfile" -w '%{http_code}' -X "$method" "$url" \
    -H "Content-Type: application/json" \
    -H "Authorization: Bearer $TOKEN" \
    -d "$body")

  printf 'ctx=%s method=%s path=%s http=%s body_size=%s\n' \
    "$ctx" "$method" "$path" "$code" "$(wc -c < "$outfile" | tr -d ' ')" \
    > "$metafile"
  printf '[%s] %s %s → HTTP %s\n' "$ctx" "$method" "$path" "$code"
}

contexts=(capital compliance enforcement exchange ledger reconciliation revenue risk routing subject transaction vault)
for ctx in "${contexts[@]}"; do run_one "$ctx" & done
wait

sleep 3

post_events=$(psql_events "SELECT count(*) FROM events;")
post_outbox=$(psql_events "SELECT count(*) FROM outbox;")
post_published=$(psql_events "SELECT count(*) FROM outbox WHERE status='published';")
printf 'POST-RUN: events=%s outbox=%s published=%s\n' \
  "$post_events" "$post_outbox" "$post_published" \
  | tee "$EVIDENCE_DIR/_post-run.txt"

printf 'DELTA: events=%d outbox=%d published=%d\n' \
  $((post_events - baseline_events)) \
  $((post_outbox - baseline_outbox)) \
  $((post_published - baseline_published)) \
  | tee "$EVIDENCE_DIR/_delta.txt"
