# AUDIT: e2e-validation

**Classification:** validation
**Runs after:** any prompt classified `validation` / `phase1.5-gate`, and against any modified file under `/scripts/validation/`, `/docs/validation/`, `/claude/guards/runtime.guard.md` §Test & E2E Validation.

## CHECKS

### A-E2E-001 — Report integrity
- `/docs/validation/e2e-validation-report.md` exists.
- Contains all 14 sections from the source prompt.
- Every test row has the §13 block (TEST NAME, STATUS, REQUEST, RESPONSE, EVENTS, KAFKA, PROJECTION, POLICY, CHAIN, NOTES).

### A-E2E-002 — No phantom PASSes
Grep for `STATUS: PASS` in `/docs/validation/`. For each hit, assert presence of `EVIDENCE:` block on next ≤10 lines. Phantom PASS = S0 finding, fail audit.

### A-E2E-003 — Script executability
- `scripts/validation/run-e2e.sh`, `failure-tests.sh`, `load-smoke.sh` exist.
- `bash -n` parses cleanly on each.
- Each script begins with `set -euo pipefail`.
- Each script has a `--dry-run` mode that exits 0 without contacting services.

### A-E2E-004 — Determinism in fixtures
Grep scripts and any `tests/validation/**` for forbidden symbols: `Guid.NewGuid`, `DateTime.UtcNow`, `Date.now`, `$RANDOM`, `uuidgen`. Any hit = S1 finding.

### A-E2E-005 — Guard back-reference
`/docs/validation/e2e-validation-report.md` MUST cite `runtime.guard.md §Test & E2E Validation` in its preamble. Missing = S2.

### A-E2E-006 — Severity hygiene
Every entry in the report's "Failures" table has one of {CRITICAL, HIGH, MEDIUM, LOW}. Ad-hoc severities = S2.

### A-E2E-007 — New rule capture
If this audit discovers a class of drift not covered by an existing guard, capture under `/claude/new-rules/{YYYYMMDD-HHMMSS}-validation.md` per $1c.

## OUTPUT
Append findings to `/claude/audits/e2e-validation.audit.output.md` with: CHECK_ID, STATUS, FILE, LINE, NOTE.
