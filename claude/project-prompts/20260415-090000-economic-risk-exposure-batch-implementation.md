# Economic / Risk — Exposure Domain-Group Batch Implementation Prompt

## CLASSIFICATION
economic-system

## CONTEXT
risk

## DOMAIN GROUP
risk

## DOMAINS
exposure

## OBJECTIVE
Run the canonical E1 → EX implementation flow against the risk domain group
(single domain: exposure) under the project's locked architecture
(S4 minimum, deterministic, layer-pure, WHYCEPOLICY / WhyceID / WhyceChain
aligned).

## EXECUTION STEPS
Per the project's stage execution model:
- E1 domain (audit-and-align existing aggregate/events/specs/errors)
- E2 commands contract
- E3 queries contract
- E4 T2E engine handlers
- E5 policy binding (Risk context)
- E6 event fabric topic map + schemas + payload mappers
- E7 projections (read model + handler + reducer + registry wiring)
- E8 API controller + request models
- E9 workflow — NOT justified for single-aggregate CRUD; outlined as deferred
- E10 observability outline
- E11 security & enforcement outline
- E12 E2E validation outline

## CONSTRAINTS
- CLASSIFICATION → CONTEXT → DOMAIN GROUP → DOMAIN preserved
  (economic-system/risk/risk/exposure — context and domain-group collapse under
  the existing three-level on-disk layout at src/domain/economic-system/risk/exposure)
- Domain layer remains zero-infrastructure
- No Guid.NewGuid / DateTime.UtcNow / random sources
- Topic naming, namespaces, contracts remain canonical
- No workflow layer (T1M) for this batch — single-aggregate reversible lifecycle

## OUTPUT FORMAT
1. Batch Summary
2. Scope Confirmation
3. Stage-by-Stage Implementation Plan
4. Files to Create / Modify
5. Domain Rules / Invariants
6. Integration Notes
7. Validation Checklist
8. Change Report
9. Risks / Gaps / Follow-up Items

## VALIDATION CRITERIA
- Correct classification / context / domain-group placement
- Single exposure domain fully staged
- No infra leakage; no non-deterministic APIs
- Invariants, errors, events, specs explicit
- Topic naming canonical; workflow intentionally deferred
- Production-ready output (no placeholder stubs)

## NOTES
Input taken from `/pipeline/execution_context.md` per $17. Archived per $2.
