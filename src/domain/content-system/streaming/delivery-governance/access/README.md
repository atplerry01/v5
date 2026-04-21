# access

## Purpose

The `access` leaf owns technical stream-access grant truth — a grant carries a mode, a validity window, and an opaque token binding, and moves through granted → restricted → revoked / expired.

It is **technical** access, not commercial entitlement.

## Aggregate root

- `StreamAccessAggregate`

## Key value objects

- `StreamAccessId`
- `StreamRef`
- `AccessMode`
- `AccessWindow`
- `TokenBinding`
- `AccessStatus` (Granted / Restricted / Revoked / Expired)

## Key events

- `StreamAccessGrantedEvent`
- `StreamAccessRestrictedEvent`
- `StreamAccessUnrestrictedEvent`
- `StreamAccessRevokedEvent`
- `StreamAccessExpiredEvent`

## Invariants and lifecycle rules

- `Grant` rejects if the access window has already expired at grant-time (shared-kernel guard).
- `Restrict` requires a non-empty reason; rejects terminal (`Revoked` / `Expired`) and already-restricted.
- `Unrestrict` is valid only from `Restricted`.
- `Revoke` requires a non-empty reason; rejects already-revoked or expired.
- `Expire` rejects already-revoked or expired.
- Terminal = `Revoked` / `Expired`.

## Owns

- Grant identity, stream binding, mode, window, token binding, status.
- Grant / restrict / unrestrict / revoke / expire transitions.

## References

- `StreamRef` — the stream this grant applies to.
- `TokenBinding` — opaque token binding; identity of the grantee is deliberately not modelled here.

## Does not own

- Whether a user has paid, subscribed, or is legally permitted — commercial/entitlement concern, lives outside `content-system`.
- Token issuance, signing, or verification — infrastructure and policy concerns.
- DRM licence issuance — policy / infrastructure.
- Playback mechanics — owned by `delivery-artifact/playback`.

## Template conformance (E1→EX `01-domain-skeleton`)

- **MUST folders** (`aggregate/`, `error/`, `event/`, `value-object/`) — present and populated.
- **WHEN-NEEDED folders**:
  - `entity/` — omitted (aggregate has no child entities with independent identity); `.gitkeep` retained.
  - `service/` — omitted (no cross-aggregate coordination required at D1); `.gitkeep` retained.
  - `specification/` — populated (`CanRevokeAccessSpecification`).
- **Lifecycle-init idempotency** (`DOM-LIFECYCLE-INIT-IDEMPOTENT-01`) — satisfied by construction: `StreamAccessAggregate.Grant(…)` is a static factory that returns a freshly-constructed instance via the private parameterless constructor. `Version` is therefore always `-1` at init time and a second initialisation cannot be dispatched on an already-loaded aggregate. No instance-method init path exists, so no explicit `Version >= 0` guard is required.
