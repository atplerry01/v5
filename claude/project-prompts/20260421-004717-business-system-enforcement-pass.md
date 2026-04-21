---
classification: business-system
context: cross-context
domain: enforcement-pass
stored_at: 2026-04-21T00:47:17Z
---

# TITLE
Business-System Enforcement Pass — Precision + Enforcement Upgrade (not a redesign)

# CONTEXT
WBSM v3 canonical execution. The business-system domain already exists across nine contexts (agreement, customer, entitlement, offering, order, pricing, provider, service, shared). Prior structural passes bound most relationships to typed VOs and placed aggregates under the canonical `classification/context/[domain-group]/domain/` nesting. This pass targets precision defects that remain: primitive IDs, duplicated concepts, weak invariants, inconsistent events, uncentralised cross-aggregate rules, and multiple paths for the same business action.

# OBJECTIVE
Reach full business-system compliance so that:
- All business meaning is unambiguous
- All aggregates enforce explicit invariants via `EnsureInvariants()` (or equivalent)
- No duplicate or competing domain concepts exist
- All domain events are consistent (`<Entity><Action>Event`, canonical-ID payloads)
- Cross-aggregate rules are explicitly modelled as DomainPolicy / DomainService
- Every business operation has a single canonical command / event path

# CONSTRAINTS
- No redesign. No new identity systems. No structural-system behavioural change. No event-schema change unless strictly required.
- Work incrementally — per aggregate or per concept.
- Do NOT merge aggregates unless duplication is proven.
- Audit first, enforce second. Fail-fast invariants; no silent correction.
- Respects all four canonical guards (constitutional, runtime, domain, infrastructure) and the WBSM v3 priority order.

# EXECUTION STEPS
1. Phase 0 — Audit: primitive IDs, duplicate concepts, weak invariants, event drift, cross-aggregate rules living inside single aggregates.
2. Phase 1 — Structural ID binding for any residual primitive identifiers.
3. Phase 2 — Semantic duplication resolution.
4. Phase 3 — Invariant formalisation inside `EnsureInvariants()`.
5. Phase 4 — Event naming + payload standardisation.
6. Phase 5 — DomainPolicy extraction for cross-aggregate/temporal rules.
7. Phase 6 — Single canonical path per business operation.
8. Post-execution audit sweep per $1b.
9. Capture any drift per $1c.

# OUTPUT FORMAT
- Punch list of primitive-ID remediations.
- Punch list of duplicate-concept resolutions.
- Punch list of aggregates that received invariants.
- Standardised event list.
- DomainPolicy / DomainService additions.
- Audit-sweep summary + any new-rules captures.

# VALIDATION CRITERIA
- Zero primitive (`Guid`, `string`) identity fields in business-system aggregates, entities, events, or VO bodies representing identity.
- Every aggregate in business-system declares explicit invariants that fail fast.
- Every event name follows `<Entity><Action>Event` and carries canonical IDs.
- No two aggregates model the same real-world concept.
- Cross-aggregate rules live in policy / service types, not scattered in aggregate branches.
- All post-execution audits pass (or emit structured new-rule captures under `/claude/new-rules/`).
