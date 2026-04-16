# Economic / Capital — Domain-Group Batch Implementation Prompt

## CLASSIFICATION
economic-system

## CONTEXT
capital

## DOMAIN GROUP
capital

## DOMAINS
account, allocation, asset, binding, pool, reserve, vault

## OBJECTIVE
Run the canonical E1 → EX implementation flow against the capital domain group
under the project's locked architecture (S4 minimum, deterministic, layer-pure,
WHYCEPOLICY / WhyceID / WhyceChain aligned).

## EXECUTION STEPS
Per the project's stage execution model (E1 domain → E2 commands → E3 queries →
E4 T2E handlers → E5 policy → E6 event fabric → E7 projections → E8 API →
E9 workflow → E10 observability → E11 security → E12 E2E validation).
E13–EX outlined only.

## CONSTRAINTS
- CLASSIFICATION → CONTEXT → DOMAIN GROUP → DOMAIN hierarchy must be preserved
- Domain layer remains zero-infrastructure
- No Guid.NewGuid / DateTime.UtcNow / random sources
- Topic naming, namespaces, contracts remain canonical
- Workflows only where justified

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
- All 7 domains addressed
- No infra leakage; no non-deterministic APIs
- Invariants, errors, events, specs are explicit
- Topic naming canonical; workflows justified where introduced
- Production-ready output (no placeholder stubs)

## NOTES
Prompt body relayed by user as IDE selection on `generic-prompt.md`; archived per $2.
