# Business-System — All-Contexts E1→EX Template 01 Alignment (Batch)

## CLASSIFICATION

- Classification: business-system
- Context: all remaining (invariant, provider, customer, entitlement, order, service, offering, pricing)
- Domain: multi-context structural refactor (follows 20260421-103840 agreement alignment)

## CONTEXT

Follow-up batch to the 2026-04-21 agreement alignment (`20260421-103840-business-system-agreement-template-alignment.md`). Extends the same E1→EX template 01 structural refactor to the remaining 8 contexts under `src/domain/business-system/`.

Pre-execution scan identified 45 aggregates carrying the identical deviation pattern uncovered in agreement:

- aggregate does not inherit `AggregateRoot`
- manual `_uncommittedEvents` / `AddEvent` / `Version++` machinery
- events lack `: DomainEvent` and `[JsonPropertyName("AggregateId")]`
- ID VOs throw raw `ArgumentException`
- custom `{X}DomainException : Exception` classes in error factories

Per-context BC counts:

- invariant: 1 BC (skeleton stub — no aggregate)
- provider: 5 BCs
- customer: 6 BCs
- entitlement: 6 BCs
- order: 6 BCs
- service: 6 BCs
- offering: 7 BCs
- pricing: 9 BCs

## OBJECTIVE

Align all 46 BCs across 8 contexts with canonical template 01 (domain-skeleton) targeting D1 (Partial) per `00-section-checklist.md`. Mechanical structural refactor only — preserve every public method signature, state transition, and validation rule.

## CONSTRAINTS

- Do NOT redesign domain logic or introduce new abstractions.
- Do NOT modify specifications (already canonical) or service stubs.
- Do NOT touch anything outside each context's root.
- Preserve every existing state transition and method signature.
- Respect pre-existing in-flight git-modified files (entitlement/usage-right, order-core, offering/bundle/catalog/package) — read from working tree and apply refactor on top.
- Follow canonical patterns from `src/domain/economic-system/capital/account/` exemplar and the already-aligned `src/domain/business-system/agreement/commitment/contract/`.

## EXECUTION STEPS

1. Use the agreement alignment output as the frozen reference pattern.
2. Wave 1 parallel (invariant + provider + customer): 3 general-purpose subagents, context-scoped.
3. Single domain build + audit-grep between waves.
4. Wave 2 parallel (entitlement + order + service).
5. Wave 3 parallel (offering + pricing).
6. Clean up business-system/shared/ reference VOs (`CustomerRef`, `OrderRef`, `PriceBookRef`, `ServiceDefinitionRef`) + `TimeWindow` — all had the same raw `ArgumentException` drift.
7. Final full-solution build + architecture test suite.
8. Update new-rules capture for the now-much-larger drift footprint (empty service/ stubs + empty entity/ folders across 46 BCs).

## OUTPUT FORMAT

- Per-wave per-BC file counts.
- Grep audit table post-refactor.
- Build + architecture tests pass/fail.
- Residual drift captured to new-rules.

## VALIDATION CRITERIA

Under `src/domain/business-system/`:

- 55 aggregates inherit `: AggregateRoot` (10 agreement + 45 remaining)
- 220 event files carry `: DomainEvent` and `[JsonPropertyName("AggregateId")]`
- 0 files contain `_uncommittedEvents`, `AddEvent`, `GetUncommittedEvents`, `Version++`
- 0 files contain raw BCL exception throws (`ArgumentException`, `ArgumentNullException`, `InvalidOperationException`, `NotImplementedException`, `NotSupportedException`)
- 0 files contain `Guid.NewGuid`, `DateTime.(Now|UtcNow|Today)`, `new Random`
- 0 custom `{X}DomainException : Exception` classes remain
- 0 `Microsoft.Extensions.DependencyInjection` imports in domain
- `dotnet build src/domain/Whycespace.Domain.csproj` → 0 warnings, 0 errors
- Architecture test suite: 72/72 pass
