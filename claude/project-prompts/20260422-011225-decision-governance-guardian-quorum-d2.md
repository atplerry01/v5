---
TITLE: Phase 2.7B — Governance / Guardian / Quorum Full D2 Implementation Topic Generation
CLASSIFICATION: decision-system
CONTEXT: governance
DOMAIN: guardian, quorum, governance-context
PROMPT_TYPE: topic-generation
DATE: 2026-04-22T01:12:25Z
---

## CONTEXT

Phase 2.7 covers the T0U End-to-End Policy Implementation. The phase-2.7.md guide defines three major blocks: 2.7A (Policy Lifecycle and Governance), 2.7B (Policy Analysis and Simulation), 2.7C (Policy Release and Enforcement). The governance context under `decision-system` contains 17 BCs, all at D1 (scaffold) level. Guardian and Quorum are the two primary domains that protect decision integrity — Guardian as the oversight actor and Quorum as the threshold enforcer. Both have stub aggregates only; no business logic is implemented.

## OBJECTIVE

Generate and create the phase-2.7b.md topic file under `claude/project-topics/v2/` as the full D2 implementation plan for the `decision-system/governance` context, with primary depth on Guardian and Quorum, covering all 17 governance BCs and all cross-cutting layers (engine, runtime, policy, API, persistence, messaging, projections, testing, observability, documentation).

## CONSTRAINTS

- WBSM v3 canonical rules apply throughout
- All guard files pre-loaded: constitutional, runtime, domain, infrastructure
- Anti-drift: no new architecture patterns, no renaming, no file moves
- Determinism required: IIdGenerator, IClock, no Guid.NewGuid, no DateTime.Now
- Layer purity: domain has zero external dependencies
- E1→EX delivery: full 18-section depth (domain + engine + runtime + API + projection + tests + infra)
- INV-IDEMPOTENT-LIFECYCLE-INIT-01: 3-layer idempotency for all lifecycle-init aggregates
- All events carry full payload with past-tense naming
- All value objects are immutable
- POL-09: Guardian and Quorum enforce multi-party approval and quorum requirements per WHYCEPOLICY

## EXECUTION STEPS

1. Load all guard files from `claude/guards/`
2. Read `claude/project-topics/v2/phase-2.7.md` as the overarching guide
3. Audit current governance BC state (all 17 BCs, current activation levels)
4. Read Guardian and Quorum README.md and stub aggregates for current state
5. Read phase-2.8b.md for format reference
6. Generate phase-2.7b.md with 31 sections covering:
   - Foundation (section 0)
   - Guardian D2 (section 1) — full aggregate, events, value objects, specifications, errors, services, invariants
   - Quorum D2 (section 2) — all threshold types, vote tally, evaluation logic
   - All remaining 15 governance BCs (sections 3–17) — full D2 each
   - Engine layer (section 18) — all command handlers
   - Policy integration (section 19)
   - Runtime integration (section 20)
   - Platform API (section 21)
   - Persistence (section 22)
   - Messaging/Kafka (section 23)
   - Projections (section 24)
   - Cross-domain integration (section 25)
   - Security hardening (section 26)
   - Observability (section 27)
   - Testing and certification (section 28)
   - Resilience validation (section 29)
   - Documentation and anti-drift (section 30)
   - Completion criteria (section 31)
7. Store prompt in `claude/project-prompts/` per $2
8. Post-execution audit sweep

## OUTPUT FORMAT

- `claude/project-topics/v2/phase-2.7b.md` — complete implementation topic
- `claude/project-prompts/20260422-011225-decision-governance-guardian-quorum-d2.md` — this prompt

## VALIDATION CRITERIA

- Topic file covers all 17 governance BCs to D2
- Guardian domain has full aggregate lifecycle (Appointed → Active → OversightRecorded → Reassigned/Retired → Archived)
- Quorum domain covers all four threshold types (Absolute, Percentage, Supermajority, Unanimous)
- All cross-cutting layers (engine through documentation) present
- Completion criteria section present
- Execution order batches defined
- No architecture drift; no new patterns introduced
- All guard constraints respected (determinism, event-first, CQRS, policy-gate)
