# observability.guard.md

## NEW RULES INTEGRATED — 2026-04-07 (NORMALIZATION)

### RULE: OBS-TRACE-01 — FULL PIPELINE TRACEABILITY
All executions MUST emit traceable metadata.

ENFORCEMENT:
- Must include: EventId, ExecutionHash, DecisionHash, PolicyVersion

---

### RULE: OBS-REPLAY-01 — REPLAY DETERMINISM SUPPORT
System MUST support deterministic replay mode.

ENFORCEMENT:
- Replay must reproduce identical: events, hashes, decisions
