# Subscription Domain

**Path:** `content-system/monetization/subscription`
**Namespace:** `Whycespace.Domain.ContentSystem.Monetization.Subscription`

## Purpose
Owns a subscriber's subscription against a plan: create, activate,
renew, cancel, expire.

## Lifecycle
```
Created → Active ⇄ Renewed(Active) → Expired / Cancelled (terminal)
```

## Events
- `SubscriptionCreatedEvent`
- `SubscriptionActivatedEvent`
- `SubscriptionRenewedEvent`
- `SubscriptionCancelledEvent`
- `SubscriptionExpiredEvent`

## Invariants
1. Subscriber and plan references required.
2. Period end strictly after period start.
3. Only Created may be Activated.
4. Only Active may be Renewed.
5. Expired/Cancelled are terminal.
