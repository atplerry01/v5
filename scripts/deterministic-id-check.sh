#!/usr/bin/env bash
# HSID v2.1 / Deterministic ID compliance check.
# Mirrors deterministic-id.guard.md A1, A5, A6.
set -euo pipefail

SCAN_DIRS=(
  "src/engines/T0U/determinism"
  "src/shared/kernel/determinism"
)

FORBIDDEN=(
  "Guid\\.NewGuid"
  "DateTime\\.Now"
  "DateTime\\.UtcNow"
  "DateTimeOffset\\.Now"
  "DateTimeOffset\\.UtcNow"
  "Random\\.Shared"
  "RandomNumberGenerator"
  "Environment\\.TickCount"
  "Stopwatch\\.GetTimestamp"
)

fail=0

for dir in "${SCAN_DIRS[@]}"; do
  if [ -d "$dir" ]; then
    for pattern in "${FORBIDDEN[@]}"; do
      if grep -RInE "$pattern" "$dir" >/dev/null 2>&1; then
        echo "FAIL: forbidden pattern '$pattern' in $dir"
        grep -RInE "$pattern" "$dir" || true
        fail=1
      fi
    done
  fi
done

# G5 — domain purity
if [ -d "src/domain" ]; then
  if grep -RInE "IDeterministicIdEngine|Whyce\\.Shared\\.Kernel\\.Determinism|TopologyCode" src/domain >/dev/null 2>&1; then
    echo "FAIL: domain layer references HSID seam"
    grep -RInE "IDeterministicIdEngine|Whyce\\.Shared\\.Kernel\\.Determinism|TopologyCode" src/domain || true
    fail=1
  fi
fi

# G6 — call-site allow-list
allowed_hits=$(grep -RInE "IDeterministicIdEngine" src 2>/dev/null \
  | grep -vE "src/(shared/kernel/determinism|engines/T0U/determinism|runtime/control-plane|platform/host/composition/runtime)/" || true)
if [ -n "$allowed_hits" ]; then
  echo "FAIL: IDeterministicIdEngine referenced outside permitted surfaces"
  echo "$allowed_hits"
  fail=1
fi

if [ "$fail" -ne 0 ]; then
  echo "HSID v2.1 compliance: FAILED"
  exit 1
fi

echo "HSID v2.1 compliance: PASSED"
