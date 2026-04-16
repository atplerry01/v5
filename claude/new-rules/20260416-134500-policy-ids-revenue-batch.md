---
classification: audits
source: /pipeline/execution_context_v1.md batch (economic-system/revenue, 2026-04-16)
severity: S0 (at first dispatch)
---

# Drift capture: missing WHYCEPOLICY bindings for 6 new revenue commands

## CLASSIFICATION
constitutional (policy authority) — follow-up backlog.

## SOURCE
Pipeline audit of the `economic-system / revenue` batch. Six new commands were introduced:

- `CreateRevenueContractCommand`
- `ActivateRevenueContractCommand`
- `TerminateRevenueContractCommand`
- `DefinePricingCommand`
- `AdjustPricingCommand`
- `ExecutePayoutCommand`

## DESCRIPTION
Per `constitutional.guard.md` PB-01..PB-04 and POL-02, every command must have a resolved, active, in-scope policy in the registry before dispatch. The batch did not touch the policy registry because the canonical registry-file location and schema was not verified during the batch (follow-up PR scope).

If any of these commands is dispatched with current state, the runtime policy middleware will BLOCK the request with an `economic.*.create_failed` / `economic.*.define_failed` / `economic.*.adjust_failed` / `economic.*.execute_failed` error before reaching the engine.

## PROPOSED RULE
Extend `src/shared/contracts/policy/` (sibling of `TodoPolicyIds.cs`) with a `RevenuePolicyIds.cs` class exposing the canonical policy IDs:

```csharp
public static class RevenuePolicyIds
{
    public const string CreateContract    = "economic.revenue.contract.create";
    public const string ActivateContract  = "economic.revenue.contract.activate";
    public const string TerminateContract = "economic.revenue.contract.terminate";
    public const string DefinePricing     = "economic.revenue.pricing.define";
    public const string AdjustPricing     = "economic.revenue.pricing.adjust";
    public const string ExecutePayout     = "economic.revenue.payout.execute";
}
```

Register the six `ICommandPolicyBinding` entries in the appropriate bootstrap (composition module + policy registry seeding path — pattern TBD via `TodoPolicyIds` reference).

## SEVERITY
S0 — commands are blocked at first dispatch until policies resolve. Non-blocking until someone actually dispatches one of the commands.
