#!/usr/bin/env bash
# HTTP utility — curl wrapper returning JSON + status code + correlation tracking
set -euo pipefail

API_BASE="${API_BASE:-http://localhost:18080}"

# POST JSON and return body + HTTP status
# Usage: http_post "/api/todo/create" '{"Title":"test","UserId":"user1"}'
# Sets: HTTP_BODY, HTTP_STATUS, CORRELATION_ID
http_post() {
    local path="$1"
    local payload="$2"
    local url="${API_BASE}${path}"
    local tmp tmp_headers
    tmp=$(mktemp)
    tmp_headers=$(mktemp)

    HTTP_STATUS=$(curl -s -o "$tmp" -D "$tmp_headers" -w '%{http_code}' \
        -X POST "$url" \
        -H "Content-Type: application/json" \
        -d "$payload") || true
    HTTP_BODY=$(cat "$tmp")

    # Extract correlationId: prefer response body, fallback to header
    CORRELATION_ID=""
    if [ -n "$HTTP_BODY" ]; then
        CORRELATION_ID=$(echo "$HTTP_BODY" | jq -r '.correlationId // empty' 2>/dev/null) || true
    fi
    if [ -z "$CORRELATION_ID" ] && [ -f "$tmp_headers" ]; then
        CORRELATION_ID=$(grep -i 'x-correlation-id' "$tmp_headers" 2>/dev/null | sed 's/.*: *//;s/\r//' | head -1) || true
    fi
    export CORRELATION_ID

    rm -f "$tmp" "$tmp_headers"
}

# GET and return body + HTTP status
# Usage: http_get "/api/todo/{id}"
# Sets: HTTP_BODY, HTTP_STATUS
http_get() {
    local path="$1"
    local url="${API_BASE}${path}"
    local tmp
    tmp=$(mktemp)

    HTTP_STATUS=$(curl -s -o "$tmp" -w '%{http_code}' \
        -X GET "$url" \
        -H "Accept: application/json") || true
    HTTP_BODY=$(cat "$tmp")
    rm -f "$tmp"
}

# Extract JSON field (requires jq)
# Usage: json_field ".todoId" "$HTTP_BODY"
json_field() {
    local field="$1"
    local json="${2:-$HTTP_BODY}"
    echo "$json" | jq -r "$field"
}
