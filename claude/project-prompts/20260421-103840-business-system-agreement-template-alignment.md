# Business-System Agreement Context — E1→EX Template 01 Alignment

## CLASSIFICATION

- Classification: business-system
- Context: agreement
- Domain: template alignment (cross-BC structural refactor)

## CONTEXT

Execute `claude/templates/delivery-pattern/_enry_prompt.md` against the `agreement` context under `src/domain/business-system/`. The context contains 10 bounded contexts organised into 3 domain-groups:

- change-control: amendment, approval, clause, renewal
- commitment: acceptance, contract, obligation, validity
- party-governance: counterparty, signature

Pre-execution scan (2026-04-21) found uniform structural deviations from canonical template `claude/templates/delivery-pattern/01-domain-skeleton.md`:

- **10 S0** — no aggregate inherits `AggregateRoot` (E1XD-AGG-INHERIT-01)
- **10 S1** — manual `_uncommittedEvents` list instead of `RaiseDomainEvent` (E1XD-AGG-RAISE-01)
- **10 S1** — missing lifecycle-init guard `Version >= 0` (E1XD-AGG-LIFECYCLE-01)
- **31 S1** — events lack `[JsonPropertyName("AggregateId")]` (E1XD-EVENT-JPN-01) and `: DomainEvent` inheritance
- **~12 S1** — value objects and entities throw raw `ArgumentException` / `ArgumentNullException` instead of `Guard.Against` (E1XD-VO-VALIDATE-01, E1XD-ERR-NOBCL-01)
- **10 S1** — custom `{X}DomainException : Exception` classes in error factories instead of canonical `DomainException` / `DomainInvariantViolationException`
- **1 S2 (drift)** — non-canonical `policy/` folder under `amendment/` containing `AmendmentApplicabilityDecision.cs` and `AmendmentApplicabilityPolicy.cs`

## OBJECTIVE

Align all 10 agreement BCs with canonical template 01 (domain-skeleton) targeting D1 (Partial) per `00-section-checklist.md`. Mechanical structural refactor only — preserve every public method signature, state transition, and validation rule.

## CONSTRAINTS

- Do NOT redesign domain logic or introduce new abstractions.
- Do NOT modify specifications (already canonical) or service stubs.
- Do NOT touch anything outside `src/domain/business-system/agreement/`.
- Preserve every existing state transition and method signature.
- Follow canonical patterns from `src/domain/economic-system/capital/account/` exemplar.

## EXECUTION STEPS

1. Load all 4 canonical guards per $1a.
2. Read templates 01 (domain-skeleton), README, 00-section-checklist.
3. Scan 10 BCs, enumerate deviations by severity/category.
4. Refactor Contract BC manually as the reference pattern.
5. Delegate remaining 9 BCs to a parallel agent using Contract as the exemplar.
6. Verify: `dotnet build` + architecture test suite (72 tests).
7. Run e1-ex-domain audit checks via Grep against refactored files.
8. Capture pre-existing drift to new-rules per $1c.
9. Store this prompt per $2.

## OUTPUT FORMAT

- List of files modified (count per BC).
- Key pattern changes applied.
- Audit grep results (all zero for negative checks).
- Build + test pass/fail.
- New-rules entries captured.

## VALIDATION CRITERIA

- 10/10 aggregates inherit `AggregateRoot`.
- 0 files contain `_uncommittedEvents`, `AddEvent`, `GetUncommittedEvents`, `Version++`.
- 0 files contain raw BCL exception throws (`ArgumentException`, `ArgumentNullException`, `InvalidOperationException`, `NotImplementedException`).
- 0 files contain `Guid.NewGuid`, `DateTime.(Now|UtcNow|Today)`, `new Random`.
- 0 files contain `Microsoft.Extensions.DependencyInjection`.
- 31/31 event files carry `[JsonPropertyName("AggregateId")]` on the aggregate-id param and inherit `: DomainEvent`.
- 0 custom `{X}DomainException : Exception` classes remain under the context.
- `dotnet build src/domain/Whycespace.Domain.csproj` succeeds with 0 warnings, 0 errors.
- Architecture test suite passes (72/72).
- No regressions in unrelated code.
