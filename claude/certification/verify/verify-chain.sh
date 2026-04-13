#!/usr/bin/env bash
# Verify chain — confirm chain block exists for correlation ID
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
source "$SCRIPT_DIR/../utils/db.sh"
source "$SCRIPT_DIR/../utils/constants.sh"

# Usage: verify-chain.sh <correlation_id>
verify_chain() {
    local correlation_id="$1"

    echo "[STEP] Verifying chain block for correlation: $correlation_id"
    log_correlation "$correlation_id"

    local count
    count=$(count_chain_blocks "$correlation_id")

    if [ "$count" -ge 1 ]; then
        echo "[PASS] Chain block exists (count=$count)"
    else
        echo "[FAIL] No chain block found for correlation_id=$correlation_id"
        return 1
    fi

    local block
    block=$(get_chain_block "$correlation_id")
    if [ -n "$block" ]; then
        echo "[PASS] Chain block data: $block"
    else
        echo "[FAIL] Chain block data is empty"
        return 1
    fi

    # Verify integrity fields are populated
    local integrity
    integrity=$(chain_query "SELECT COUNT(*) FROM whyce_chain WHERE correlation_id = '${correlation_id}' AND event_hash != '' AND decision_hash != '' AND previous_block_hash != '';")
    if [ "$integrity" -ge 1 ]; then
        echo "[PASS] Chain block integrity fields populated"
    else
        echo "[FAIL] Chain block integrity fields incomplete"
        return 1
    fi

    return 0
}

if [[ "${BASH_SOURCE[0]}" == "${0}" ]]; then
    verify_chain "$@"
fi
