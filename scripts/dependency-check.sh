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
#
# platform/host is the composition root and carries DG-R5-EXCEPT-01: it MAY
# reference runtime, engines, systems, projections for DI registration only.
# It MUST NOT reference Whycespace.Domain — see DG-R5-HOST-DOMAIN-FORBIDDEN
# (Phase 1.5 §5.1.1 Step C).
declare -A ALLOWED=(
  [shared]=""
  [domain]="Whycespace.Shared"
  [engines]="Whycespace.Domain Whycespace.Shared"
  [runtime]="Whycespace.Engines Whycespace.Domain Whycespace.Shared"
  [systems]="Whycespace.Runtime Whycespace.Shared"
  [projections]="Whycespace.Domain Whycespace.Shared"
  [platform_api]="Whycespace.Systems Whycespace.Shared"
  [platform_host]="Whycespace.Api Whycespace.Shared Whycespace.Runtime Whycespace.Engines Whycespace.Systems Whycespace.Projections"
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
for f in "$SRC"/platform/api/**/*.csproj;  do check_csproj platform_api  "$f"; done
for f in "$SRC"/platform/host/**/*.csproj; do check_csproj platform_host "$f"; done

# ---------- C1.host-domain: explicit DG-R5-HOST-DOMAIN-FORBIDDEN ----------
# Belt-and-braces precise check independent of the ALLOWED matrix above.
HOST_CSPROJ="$SRC/platform/host/Whycespace.Host.csproj"
if [ -f "$HOST_CSPROJ" ]; then
  if grep -qE 'ProjectReference Include="[^"]*Whycespace\.Domain\.csproj"' "$HOST_CSPROJ"; then
    report "DG-R5-HOST-DOMAIN-FORBIDDEN: $HOST_CSPROJ references Whycespace.Domain.csproj"
  fi
fi

# ---------- C2: using directive leakage ----------
scan_using() {
  local path="$1" forbidden="$2" label="$3"
  [ -d "$path" ] || return 0
  local hits
  hits="$(grep -RInE \
            --include='*.cs' \
            --exclude-dir=obj --exclude-dir=bin \
            "^using\s+($forbidden)" "$path" 2>/dev/null || true)"
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
scan_using "$SRC/projections" "Whyce\.Engines|Whyce\.Runtime|Whyce\.Systems|Whyce\.Platform|Whycespace\.Domain" "projections leakage"

# platform/host: DG-R5-EXCEPT-01 permits runtime/engines/systems/projections
# usings for DI/adapter wiring. DG-R5-HOST-DOMAIN-FORBIDDEN forbids any
# Whycespace.Domain.* import at any depth.
scan_using "$SRC/platform/host" "Whycespace\.Domain" "DG-R5-HOST-DOMAIN-FORBIDDEN host→domain"

# Phase 1.5 §5.1.2 Step C-G (BPV-D01 hardening): the C2 `using` scan above
# only catches `^using Whycespace.Domain.*` lines. It does NOT catch:
#   (a) fully-qualified type expressions, e.g. typeof(Whycespace.Domain.X.Y)
#   (b) namespace aliases, e.g. `using DomainEvents = Whycespace.Domain.X.Y;`
# Eleven live BPV-D01 sites bypassed the predicate via these two forms and
# survived §5.1.1 PASS unnoticed. The strengthened scan below matches any
# textual `Whycespace.Domain.` occurrence in `*.cs` source under
# `src/platform/host/**`, excluding pure-comment lines (//, ///, * inside
# block comments). It is intentionally narrower than free-text grep:
#   - skips lines whose first non-whitespace characters are `//` or `*`
#   - skips obj/bin
host_fq_hits="$(grep -RInE \
                  --include='*.cs' \
                  --exclude-dir=obj --exclude-dir=bin \
                  'Whycespace\.Domain\.' \
                  "$SRC/platform/host" 2>/dev/null \
                | grep -vE ':[[:space:]]*(//|\*)' \
                || true)"
if [ -n "$host_fq_hits" ]; then
  while IFS= read -r line; do
    report "DG-R5-HOST-DOMAIN-FORBIDDEN (fully-qualified or alias): $line"
  done <<< "$host_fq_hits"
fi
# platform/api remains restricted to systems + shared.
scan_using "$SRC/platform/api"  "Whyce\.Runtime|Whyce\.Engines|Whycespace\.Domain|Whyce\.Projections" "platform/api leakage"

# ---------- C4: adapter leakage ----------
# C4 detects infrastructure-style adapter classes living outside the two
# allowed roots. It must NOT flag domain bounded contexts that legitimately
# use the word "adapter" as a business concept (e.g.
# src/domain/business-system/integration/adapter/**, where Adapter is a
# first-class domain aggregate, not an infrastructure adapter). The
# canonical domain path segment is /adapter/ (lowercase, directory-level);
# whitelist any *Adapter*.cs file whose path contains /domain/ AND a
# /adapter/ directory segment. All other *Adapter*.cs files outside the
# two allowed infrastructure roots remain S0 violations.
# Phase 1.5 §5.1.1 Step H3 (2026-04-08): false-positive narrowing.
while IFS= read -r f; do
  case "$f" in
    */platform/host/adapters/*) ;;
    */infrastructure/*) ;;
    */domain/*/adapter/*) ;;
    *) report "adapter outside allowed paths: $f" ;;
  esac
done < <(find "$SRC" -type f -name '*Adapter*.cs' 2>/dev/null)

# ---------- C5: shared kernel I/O ----------
if [ -d "$SRC/shared" ]; then
  io_hits="$(grep -RInE \
              --include='*.cs' \
              --exclude-dir=obj --exclude-dir=bin \
              'Npgsql|Confluent\.Kafka|StackExchange\.Redis|Minio|System\.Net\.Http|System\.IO\.File' \
              "$SRC/shared" 2>/dev/null || true)"
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
