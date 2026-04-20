# integrity (SCAFFOLD — pending implementation)

## Purpose

The `integrity` leaf owns the **attestation lifecycle** for a document/file's integrity — computing a checksum, comparing it to the declared checksum, and issuing, refreshing, or revoking attestations.

## Owns

- Integrity attestation identity, target ref (DocumentFileRef), declared checksum, computed checksum, verifier identity, verification timestamp, status (Pending / Verified / Mismatched / Revoked).
- Attest / re-attest / revoke / supersede transitions.

## Does not own

- The file bytes — owned by `document/core-object/file`.
- The checksum algorithm implementation — infrastructure.
- The `DocumentFileIntegrityStatus` VO currently living on `DocumentFile` — that stays as a cached/denormalized view updated in response to attestation events (§DF-05 OPT-B).

## Boundary notes

Per §DF-05: DocumentFile retains `IntegrityStatus` as a read-side cache. The verification LIFECYCLE, however, is owned here. Future cleanup: move `DocumentFileIntegrityVerifiedEvent` handling logic off `DocumentFile` and onto this aggregate.

## Status

SCAFFOLD only in P2.6.CS.3.
