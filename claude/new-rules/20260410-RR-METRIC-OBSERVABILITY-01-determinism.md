CLASSIFICATION: determinism
SOURCE: deep-sweep-20260410.audit.output.md (subagent: determinism/engine/runtime)
SEVERITY: S2

DESCRIPTION:
`Stopwatch.GetTimestamp()` / `Stopwatch.GetElapsedTime()` are currently in
practice used for observability histograms (e.g. ChainAnchorService.cs:101,109,119)
but determinism.guard does not explicitly carve them out. Reviewers must each
re-derive the rule that timing-for-metrics is permitted while timing-as-hash-input
is forbidden.

PROPOSED_RULE:
Amend determinism.guard DET-EXCEPTION block to add:
"Stopwatch.GetTimestamp / GetElapsedTime are PERMITTED solely for observability
instrumentation (latency histograms, counters, traces). The resulting value MUST
NOT flow into ExecutionHash, deterministic IDs, sequence seeds, chain block IDs,
or any persisted event payload. Lint: any data flow from Stopwatch to a hash/id
constructor is a DET violation."

PROMOTION TARGET: claude/guards/determinism.guard.md
