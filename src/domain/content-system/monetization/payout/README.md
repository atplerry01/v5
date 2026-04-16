# Content Payout Domain

**Path:** `content-system/monetization/payout`
**Namespace:** `Whycespace.Domain.ContentSystem.Monetization.Payout`

## Purpose
Owns a payout computation for a single content item: calculate, approve,
settle, or fail. `PayoutShareAllocator` splits a gross amount among
beneficiary shares in a deterministic, round-stable way.

## Lifecycle
```
Calculated → Approved → Settled (terminal)
          ↘ Failed (terminal)
```

## Events
- `ContentPayoutCalculatedEvent`
- `ContentPayoutApprovedEvent`
- `ContentPayoutSettledEvent`
- `ContentPayoutFailedEvent`

## Invariants
1. Content reference required.
2. Gross amount > 0; currency is ISO 4217.
3. Shares sum to exactly 1.
4. Status transitions: Calculated → Approved → Settled; Failed from any non-settled.
