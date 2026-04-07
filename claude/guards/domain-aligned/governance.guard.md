# governance.guard.md

## NEW RULES INTEGRATED — 2026-04-07 (NORMALIZATION)

### RULE: POLICY-ENFORCEMENT-01 — POLICY MUST GATE EXECUTION
Policy MUST execute BEFORE any domain or engine execution.

ENFORCEMENT:
- PolicyMiddleware must run BEFORE aggregate load
- Deny MUST terminate execution immediately
- No bypass allowed from workflow or runtime

---

### RULE: POLICY-CHAIN-01 — POLICY DECISION MUST BE ANCHORED
Every policy decision MUST be written to WhyceChain.

ENFORCEMENT:
- DecisionHash must be generated deterministically
- Chain anchoring must occur BEFORE Kafka publish

---

### RULE: POLICY-DETERMINISM-01 — POLICY DECISION DETERMINISTIC
Policy evaluation MUST produce deterministic outputs.

ENFORCEMENT:
- No timestamp/random input into policy hashing
- Same input MUST produce same DecisionHash
