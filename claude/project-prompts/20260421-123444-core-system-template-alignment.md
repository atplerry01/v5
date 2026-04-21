# Core-System — E1→EX Template 01 Alignment

## CLASSIFICATION

- Classification: core-system
- Context: all (command, event, financialcontrol, reconciliation, state, temporal)
- Domain: multi-context structural refactor (third classification aligned today)

## CONTEXT

Third classification aligned with E1→EX template 01. Follows `20260421-103840` (business-system agreement), `20260421-112627` (business-system batch), `20260421-122210` (structural-system). Pre-execution scan identified:

- 31 aggregates across 31 BCs, 0 inheriting `AggregateRoot`
- 7 BCs with manual `_uncommittedEvents` machinery (command-definition, command-envelope, command-routing, event-definition, event-envelope, event-stream, state-transition) + 1 bonus find (state-snapshot, subagent discovered during refactor)
- 1 custom `EventStreamDomainException : Exception` class
- 15 raw BCL throws across various VOs and aggregates
- 24 MIN near-skeleton BCs (aggregate shell exists but no event raising yet)

## OBJECTIVE

Align all 31 core-system BCs with canonical template 01 targeting D1 (Partial). FULL refactor for BCs with event-sourcing code; MIN refactor for near-skeletons (inherit `AggregateRoot`, add version guard, migrate events, clean stubs).

## CONSTRAINTS

- Do NOT redesign domain logic or introduce new abstractions.
- Do NOT invent events, state transitions, or new methods.
- Do NOT modify specifications; leave stub services alone UNLESS they're being deleted as drift.
- Preserve every existing method signature and validation rule.
- 3-level topology (`core-system/{context}/{bc}/`).

## EXECUTION STEPS

1. Scope scan: classify each BC as FULL (has `_uncommittedEvents`) or MIN (near-skeleton).
2. Launch 3 parallel general-purpose subagents split by context to balance load:
   - Agent A: `command/*` + `event/*` (9 BCs — 6 FULL + 3 MIN)
   - Agent B: `state/*` + `temporal/*` (11 BCs — 1 FULL declared + 1 FULL discovered + 9 MIN)
   - Agent C: `financialcontrol/*` + `reconciliation/*` (11 BCs — all MIN)
3. Verify `dotnet build` 0/0 + 72/72 architecture tests.
4. Store this prompt per $2.

## OUTPUT

- Per-group BC counts + event counts.
- Audit grep across core-system.
- Build + test verification.

## VALIDATION CRITERIA

Under `src/domain/core-system/`:

- 31/31 aggregates inherit `: AggregateRoot`
- 0 files contain `_uncommittedEvents`, `AddEvent`, `GetUncommittedEvents`
- 0 raw BCL exception throws
- 0 custom `{X}DomainException : Exception` classes
- 0 `Guid.NewGuid` / `DateTime.*` / `new Random`
- Every event record inherits `: DomainEvent`; every event with an aggregate-id param carries `[property: JsonPropertyName("AggregateId")]`
- `dotnet build src/domain/Whycespace.Domain.csproj` → 0 warnings, 0 errors
- Architecture tests → 72/72 pass

## NOTES

- Agent B discovered a second FULL BC (`state/state-snapshot`) beyond the single one declared in the initial scope (`state/state-transition`) — had manual `_domainEvents` and private ctor machinery. Refactored in same pass.
- MIN BCs have parameterless `{X}CreatedEvent` records (no aggregate-id param), so `JsonPropertyName("AggregateId")` is not applicable to those.
- `financialcontrol/reserve-control` and `financialcontrol/global-invariant` showed transient incremental-build errors during parallel subagent execution that resolved on full rebuild.
