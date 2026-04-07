#!/usr/bin/env bash
# WHYCESPACE — dependency graph check
# Enforces R1–R7 from claude/guards/dependency-graph.guard.md
# Exit non-zero on any violation.

set -u
shopt -s globstar nullglob

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
SRC="$ROOT/src"
violations=0

report() {
  echo "VIOLATION: $1"
  violations=$((violations + 1))
}

# ---------- C1: project reference graph ----------
# layer => allowed references (space separated)
declare -A ALLOWED=(
  [shared]=""
  [domain]="Whycespace.Shared"
  [engines]="Whycespace.Domain Whycespace.Shared"
  [runtime]="Whycespace.Engines Whycespace.Domain Whycespace.Shared"
  [systems]="Whycespace.Runtime Whycespace.Shared"
  [projections]="Whycespace.Domain Whycespace.Shared"
  [platform]="Whycespace.Systems Whycespace.Shared Whycespace.Api"
)

check_csproj() {
  local layer="$1" file="$2"
  local allowed="${ALLOWED[$layer]}"
  while IFS= read -r ref; do
    [ -z "$ref" ] && continue
    local name
    name="$(basename "$ref" .csproj)"
    if ! echo " $allowed " | grep -q " $name "; then
      report "$file references $name (layer=$layer not allowed)"
    fi
  done < <(grep -oE 'ProjectReference Include="[^"]+"' "$file" \
            | sed -E 's/.*Include="([^"]+)".*/\1/')
}

for f in "$SRC"/shared/**/*.csproj;       do check_csproj shared       "$f"; done
for f in "$SRC"/domain/**/*.csproj;       do check_csproj domain       "$f"; done
for f in "$SRC"/engines/**/*.csproj;      do check_csproj engines      "$f"; done
for f in "$SRC"/runtime/**/*.csproj;      do check_csproj runtime      "$f"; done
for f in "$SRC"/systems/**/*.csproj;      do check_csproj systems      "$f"; done
for f in "$SRC"/projections/**/*.csproj;  do check_csproj projections  "$f"; done
for f in "$SRC"/platform/**/*.csproj;     do check_csproj platform     "$f"; done

# ---------- C2: using directive leakage ----------
scan_using() {
  local path="$1" forbidden="$2" label="$3"
  [ -d "$path" ] || return 0
  local hits
  hits="$(grep -RInE "^using\s+($forbidden)" "$path" 2>/dev/null || true)"
  if [ -n "$hits" ]; then
    while IFS= read -r line; do
      report "$label: $line"
    done <<< "$hits"
  fi
}

scan_using "$SRC/domain"      "Whyce\.Runtime|Whyce\.Engines|Whyce\.Systems|Whyce\.Platform|Whyce\.Infrastructure|Whyce\.Projections" "domain leakage"
scan_using "$SRC/engines"     "Whyce\.Runtime|Whyce\.Systems|Whyce\.Platform|Whyce\.Infrastructure|Whyce\.Projections" "engines leakage"
scan_using "$SRC/runtime"     "Whyce\.Systems|Whyce\.Platform|Whyce\.Projections" "runtime leakage"
scan_using "$SRC/systems"     "Whyce\.Engines|Whyce\.Platform|Whyce\.Infrastructure|Whyce\.Projections" "systems leakage"
scan_using "$SRC/platform"    "Whyce\.Runtime|Whyce\.Engines|Whyce\.Domain|Whyce\.Projections" "platform leakage"
scan_using "$SRC/projections" "Whyce\.Engines|Whyce\.Runtime|Whyce\.Systems|Whyce\.Platform" "projections leakage"

# ---------- C4: adapter leakage ----------
while IFS= read -r f; do
  case "$f" in
    */platform/host/adapters/*) ;;
    */infrastructure/*) ;;
    *) report "adapter outside allowed paths: $f" ;;
  esac
done < <(find "$SRC" -type f -name '*Adapter*.cs' 2>/dev/null)

# ---------- C5: shared kernel I/O ----------
if [ -d "$SRC/shared" ]; then
  io_hits="$(grep -RInE 'Npgsql|Confluent\.Kafka|StackExchange\.Redis|Minio|System\.Net\.Http|System\.IO\.File' "$SRC/shared" 2>/dev/null || true)"
  if [ -n "$io_hits" ]; then
    while IFS= read -r line; do
      report "shared kernel I/O leak: $line"
    done <<< "$io_hits"
  fi
fi

echo
echo "=== DEPENDENCY GRAPH CHECK ==="
echo "Violations: $violations"
if [ "$violations" -gt 0 ]; then
  echo "Status: FAIL"
  exit 1
fi
echo "Status: PASS"
exit 0
