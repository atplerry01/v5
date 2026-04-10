CLASSIFICATION: guards
SOURCE: deep-sweep-20260410.audit.output.md (subagent: tests/infra/kafka/platform)
SEVERITY: S0

DESCRIPTION:
stub-detection.guard STUB-R4 forbids silent exception swallowing, but the rule
relies on human review. TodoController.cs:99 contains a bare `catch { }` that
hides a JSON parse failure on projection state — exactly the failure mode the
rule exists to prevent. The rule needs a mechanical enforcement seam.

PROPOSED_RULE:
Add to STUB-R4 a CI grep gate:
`grep -rEn 'catch\s*(\([^)]*\))?\s*\{\s*\}' src/platform/api/ src/runtime/ src/engines/`
must return zero hits. Empty catches in tests/ are permitted only when the
preceding line declares `// expected:` with the exception type.

PROMOTION TARGET: claude/guards/stub-detection.guard.md (and CI workflow)
