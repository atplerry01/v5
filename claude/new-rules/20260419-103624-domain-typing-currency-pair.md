---
captured: 2026-04-19T10:36:24
captured-by: economic-vo-hardening session (post-execution audit sweep per $1b)
type: domain
severity: S1
status: open
---

# CurrencyPair throws BCL ArgumentException directly (D-ERR-TYPING-01)

## CLASSIFICATION

Domain layer purity violation — `domain.guard.md` rule **D-ERR-TYPING-01** (Framework Exception Types Forbidden in Domain).

## SOURCE

Discovered during the post-execution audit sweep on 2026-04-19 of the economic-system VO hardening session.

## DESCRIPTION

[src/domain/economic-system/exchange/fx/value-object/CurrencyPair.cs](../../src/domain/economic-system/exchange/fx/value-object/CurrencyPair.cs) lines 12–27 directly throw `System.ArgumentException`:

```csharp
if (string.IsNullOrWhiteSpace(baseCurrency.Code))
    throw new ArgumentException("Base currency code must not be empty.", nameof(baseCurrency));

if (string.IsNullOrWhiteSpace(quoteCurrency.Code))
    throw new ArgumentException("Quote currency code must not be empty.", nameof(quoteCurrency));

if (string.CompareOrdinal(baseCurrency.Code, quoteCurrency.Code) >= 0)
    throw new ArgumentException(
        $"CurrencyPair invariant violated: Base ('{baseCurrency.Code}') must precede Quote " +
        $"('{quoteCurrency.Code}') ordinally. ...",
        nameof(baseCurrency));
```

Per `domain.guard.md` D-ERR-TYPING-01, files under `src/domain/**` MUST NOT directly throw `System.ArgumentException`. They MUST use `Guard.Against(...)` from the shared kernel or a context-specific `{Bc}Errors` static factory.

## PROPOSED_RULE

Replace the three `throw new ArgumentException(...)` statements with `Guard.Against(...)` calls. The shared-kernel `Guard.Against(bool, string)` raises `DomainInvariantViolationException`. Example refactor:

```csharp
public CurrencyPair(Currency baseCurrency, Currency quoteCurrency)
{
    Guard.Against(string.IsNullOrWhiteSpace(baseCurrency.Code), "CurrencyPair base currency code must not be empty.");
    Guard.Against(string.IsNullOrWhiteSpace(quoteCurrency.Code), "CurrencyPair quote currency code must not be empty.");
    Guard.Against(string.CompareOrdinal(baseCurrency.Code, quoteCurrency.Code) >= 0,
        $"CurrencyPair invariant violated: Base ('{baseCurrency.Code}') must precede Quote ('{quoteCurrency.Code}') ordinally.");
    BaseCurrency = baseCurrency;
    QuoteCurrency = quoteCurrency;
}
```

Before fix: grep callers for `catch (ArgumentException)` on CurrencyPair construction sites. Update any exception-type-specific catches to `DomainInvariantViolationException` / `DomainException`. The platform's `DomainExceptionHandler` already maps `DomainException` to HTTP 400.

## SEVERITY

**S1** — domain purity constraint, isolated to one VO, not blocking economic core operation but a documented anti-pattern.

## OUT-OF-SCOPE JUSTIFICATION

Pre-existing in the codebase prior to the 2026-04-19 economic-VO hardening session. The session's scope was the five `Money` primitive VOs under `src/domain/shared-kernel/primitive/money/`, not the broader audit of all economic VOs. Deferred for follow-up sweep.

## PROMOTION CANDIDATE

This is an enforcement of an existing canonical guard rule, not a new rule. Promotion path: include in the next domain-purity sweep PR alongside any other `D-ERR-TYPING-01` violations discovered by `grep -rEn 'throw new (ArgumentException|ArgumentNullException|...)' src/domain/`.
