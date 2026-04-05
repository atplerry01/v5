#!/bin/bash
set -euo pipefail

API_BASE="${API_BASE:-http://localhost:5000}"
PROMETHEUS_URL="${PROMETHEUS_URL:-http://localhost:9090}"
GRAFANA_URL="${GRAFANA_URL:-http://localhost:3000}"

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

PASS=0
FAIL=0

check() {
    local name="$1"
    local url="$2"
    local expected_status="${3:-200}"

    local status
    status=$(curl -s -o /dev/null -w "%{http_code}" --max-time 5 "$url" 2>/dev/null || echo "000")

    if [ "$status" = "$expected_status" ]; then
        echo -e "${GREEN}[PASS]${NC} $name (HTTP $status)"
        PASS=$((PASS + 1))
    else
        echo -e "${RED}[FAIL]${NC} $name (expected HTTP $expected_status, got HTTP $status)"
        FAIL=$((FAIL + 1))
    fi
}

echo "============================================"
echo "  WHYCESPACE INFRASTRUCTURE VALIDATION"
echo "============================================"
echo ""

echo "--- Step 1: Health Endpoint ---"
check "Health endpoint" "$API_BASE/health"

echo ""
echo "--- Step 2: Service Health Detail ---"
HEALTH_RESPONSE=$(curl -s --max-time 10 "$API_BASE/health" 2>/dev/null || echo "{}")
echo "$HEALTH_RESPONSE" | python3 -m json.tool 2>/dev/null || echo "$HEALTH_RESPONSE"

OVERALL_STATUS=$(echo "$HEALTH_RESPONSE" | python3 -c "import sys,json; print(json.load(sys.stdin).get('status','UNKNOWN'))" 2>/dev/null || echo "UNKNOWN")
if [ "$OVERALL_STATUS" = "HEALTHY" ]; then
    echo -e "${GREEN}[PASS]${NC} All services healthy"
    PASS=$((PASS + 1))
else
    echo -e "${RED}[FAIL]${NC} Overall status: $OVERALL_STATUS"
    FAIL=$((FAIL + 1))
fi

echo ""
echo "--- Step 3: Metrics Endpoint ---"
METRICS_RESPONSE=$(curl -s --max-time 5 "$API_BASE/metrics" 2>/dev/null || echo "")
if echo "$METRICS_RESPONSE" | grep -q "^# HELP\|^# TYPE"; then
    echo -e "${GREEN}[PASS]${NC} Metrics endpoint returns Prometheus format"
    PASS=$((PASS + 1))
else
    echo -e "${RED}[FAIL]${NC} Metrics endpoint missing or invalid format"
    FAIL=$((FAIL + 1))
fi

echo ""
echo "--- Step 4: Prometheus ---"
check "Prometheus reachable" "$PROMETHEUS_URL/-/healthy"

echo ""
echo "--- Step 5: Grafana ---"
check "Grafana reachable" "$GRAFANA_URL/api/health"

echo ""
echo "============================================"
echo "  RESULTS: $PASS passed, $FAIL failed"
echo "============================================"

if [ "$FAIL" -gt 0 ]; then
    echo -e "${RED}BLOCKED — $FAIL check(s) failed${NC}"
    exit 1
else
    echo -e "${GREEN}READY FOR PHASE 1 VALIDATION${NC}"
    exit 0
fi
