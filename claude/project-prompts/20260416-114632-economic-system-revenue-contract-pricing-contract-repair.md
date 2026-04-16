---
classification: economic-system
context: revenue
domains: [contract, pricing]
type: contract-repair-patch
stored: 2026-04-16T11:46:32Z
follows: claude/audits/economic-system-compliance-audit.recovery.certification.output.md
related: claude/new-rules/20260416-102951-audits.md
scope: surgical (engines + controllers in revenue/{contract,pricing}; no domain or runtime edits)
---

# TITLE
Contract Repair Patch — economic-system / revenue / {contract, pricing}

# CONTEXT
Resolves S0 build-blocking contract drift previously captured in `claude/new-rules/20260416-102951-audits.md`. Two issues:
1. **C1 — IAggregateStore** referenced by `ActivateRevenueContractHandler`, `TerminateRevenueContractHandler`, `AdjustPricingHandler`. The type does not exist anywhere under `src/`. Per `runtime.guard.md` lines 75 / 2138, `IAggregateStore` is a runtime-internal type — engines must access it via `IEngineContext.LoadAggregateAsync(...)`, never inject it directly.
2. **C2 — CommandAck arity** mismatch in 5 controller call sites (`ContractController` × 3, `PricingController` × 2). Canonical record is `public sealed record CommandAck(string Status);` — single parameter.

# CONSTRAINTS
- ALLOWED: `src/engines/T2E/economic/revenue/{contract,pricing}/**`, controller call sites for `CommandAck`, `using` statements, DI registration.
- FORBIDDEN: `src/domain/**`, runtime contract changes, business logic, policy logic, API redesign.

# EXECUTION STEPS
1. Refactor 3 handlers to drop `IAggregateStore` and use `(X)await context.LoadAggregateAsync(typeof(X))`.
2. Drop the second argument from 5 `new CommandAck(...)` call sites.
3. `dotnet build` → green; `dotnet test` → audit-domain integration tests now executable.

# OUTPUT FORMAT
- 5 edited files (3 handlers + 2 controllers)
- Build & test verification
- Re-check audit-domain reducer + handler tests pass

# VALIDATION CRITERIA
Build PASS, audit reducer + handler tests pass, no new audit violations, no domain edits.

# ORIGINAL PROMPT
(Verbatim contract repair batch issued 2026-04-16; see conversation.)
