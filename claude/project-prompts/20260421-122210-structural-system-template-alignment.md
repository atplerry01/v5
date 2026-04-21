# Structural-System — E1→EX Template 01 Alignment

## CLASSIFICATION

- Classification: structural-system
- Context: all (cluster, structure, humancapital, contracts, invariant)
- Domain: multi-context structural refactor (same pattern as business-system 2026-04-21)

## CONTEXT

Follow-up to the business-system template-alignment passes (`20260421-103840`, `20260421-112627`) applying the same E1→EX template 01 mechanical refactor to `structural-system`. Drift scan (2026-04-21T11:40) identified:

- 24 aggregates across 24 BCs, 0 currently inheriting `AggregateRoot`
- 14 aggregates with manual `_uncommittedEvents` machinery (cluster group + 4 structure BCs)
- 9 raw BCL throws — 6 in `contracts/references/` shared refs, 3 in `structure/relationship-rules/` skeleton BCs
- 0 custom `{X}DomainException : Exception` classes (structural-system was cleaner than business-system in this dimension)

## OBJECTIVE

Align every structural-system BC with real aggregate content against canonical template 01 (domain-skeleton) targeting D1 (Partial). Pre-D1 skeleton BCs (humancapital 12 BCs, relationship-rules, reference-vocabularies, contracts/references non-VO types, invariant/economic-binding) get the minimal structural cleanup plus `: AggregateRoot` inheritance where an aggregate shell exists.

## CONSTRAINTS

- Do NOT redesign domain logic or introduce new abstractions.
- Do NOT modify specifications; leave stub services alone UNLESS they're being deleted as drift.
- Preserve every existing method signature and validation rule.
- 3-level topology throughout (`structural-system/{context}/{bc}/`) — no domain-group layer.

## EXECUTION STEPS

1. Scope scan: 3 groups — cluster (8 BCs, full refactor), structure (4 real + 2 skeleton), humancapital (12 BCs minimal).
2. Launch 3 parallel general-purpose subagents, one per group.
3. After all 3 complete: fix the 6 `contracts/references/` shared ref VOs to use `Guard.Against`.
4. Verify: `dotnet build` 0/0 + 72/72 architecture tests pass.
5. Store this prompt per $2.

## OUTPUT

- Per-group report (BCs refactored, events migrated, cleanup).
- Grep audit across structural-system.
- Build + test verification.

## VALIDATION CRITERIA

Under `src/domain/structural-system/`:

- 24 aggregates inherit `: AggregateRoot` (8 cluster + 4 structure + 12 humancapital)
- 0 files contain `_uncommittedEvents`, `AddEvent`, `GetUncommittedEvents`
- 0 files contain raw BCL throws in canonical BCs (9 remain only in pre-D1 skeleton BCs — captured to new-rules if desired)
- 0 custom `{X}DomainException : Exception` classes
- 0 `Guid.NewGuid` / `DateTime.*` / `new Random` in canonical BCs
- Every event record inherits `: DomainEvent`; every event with an aggregate-id param carries `[property: JsonPropertyName("AggregateId")]`
- `dotnet build src/domain/Whycespace.Domain.csproj` → 0 warnings, 0 errors
- Architecture tests → 72/72 pass

## NOTES

- 3 non-VO files under `contracts/references/` (`IStructuralParentLookup`, `IStructuralRelationshipPolicy`, `StructuralParentState`) were accidentally overwritten during a bulk-rewrite of reference VOs and had to be reconstructed from usage patterns in callers (aggregates + specifications). Final content matches their pre-existing contracts.
- `structure/reference-vocabularies/` and `structure/relationship-rules/` are skeleton BCs without canonical `aggregate/` folders — left untouched beyond necessary compile fixes.
- `invariant/economic-binding/` has no aggregate, similar to `business-system/invariant/economic-attribution/policy/` — skeleton stub, skipped.
