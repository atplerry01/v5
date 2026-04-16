# Certification Domain

**Path:** `content-system/learning/certification`
**Namespace:** `Whycespace.Domain.ContentSystem.Learning.Certification`

## Purpose
Records an issued course certification for a holder, with validity
window, renewal, revocation, and expiry.

## Lifecycle
```
Issued ── Renew ──► Renewed ──► ...
  │                    │
  ├── Revoke ──► Revoked (terminal)
  └── Expire ──► Expired (terminal)
```

## Events
- `CertificationIssuedEvent`
- `CertificationRenewedEvent`
- `CertificationRevokedEvent`
- `CertificationExpiredEvent`

## Invariants
1. Holder & course refs required.
2. Validity end strictly after start.
3. Revoked cannot be renewed.
4. Revoked/Expired are terminal.
