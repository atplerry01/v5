# Entitlement Domain

**Path:** `content-system/monetization/entitlement`
**Namespace:** `Whycespace.Domain.ContentSystem.Monetization.Entitlement`

## Purpose
Models a holder's entitlement: tier, validity window, and scoped
permissions. Supports extension, downgrade, and revocation.

## Lifecycle
```
Granted → Extended        → Revoked (terminal)
       → Downgraded       → Revoked (terminal)
```

## Events
- `EntitlementGrantedEvent`
- `EntitlementExtendedEvent`
- `EntitlementDowngradedEvent`
- `EntitlementRevokedEvent`

## Invariants
1. Holder required.
2. Validity end strictly after start; extensions only forward in time.
3. Downgrades require strictly lower tier.
4. Revoked is terminal.
