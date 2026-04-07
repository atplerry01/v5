# identity.guard.md

## NEW RULES INTEGRATED — 2026-04-07 (NORMALIZATION)

### RULE: ID-POLICY-01 — IDENTITY MUST BE POLICY-RESOLVED
All identity context MUST be resolved BEFORE policy evaluation.

ENFORCEMENT:
- IdentityMiddleware MUST run before PolicyMiddleware
- Identity context must include: subjectId, roles, trustScore, verificationStatus

---

### RULE: ID-DETERMINISM-01 — IDENTITY DETERMINISM
Identity resolution MUST be deterministic across replay.

ENFORCEMENT:
- No random ID/session generation in runtime flow
- Identity must be derived from request context deterministically
