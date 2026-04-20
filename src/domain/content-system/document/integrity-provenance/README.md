# document / integrity-provenance

## Purpose

Groups **attestation and provenance** aggregates for the document context. Integrity attestations verify that a document/file's bytes match their declared state; provenance records the chain of custody and origin claims.

## Why this group exists

Integrity and provenance are attestation classes — they are NOT core objects, NOT ingress, NOT state transitions, NOT governance decisions. They record verifiable claims about documents. Isolating them here prevents integrity logic from bleeding back into `core-object/file` (see §DF-05).

## Leaf domains

- `integrity/` — (SCAFFOLD) integrity attestation lifecycle. Aggregate that verifies a file's computed checksum matches its declared checksum, tracks verification time and verifier identity, and issues attestation events.
- `provenance/` — (SCAFFOLD) provenance-chain aggregate. Records origin claims, upstream-source attribution, and custody transitions.

## Boundary notes

- The `core-object/file.DocumentFileIntegrityStatus` VO and `DocumentFileIntegrityVerifiedEvent` stay on `DocumentFile` as cached/denormalized view state. The verification LIFECYCLE (compute, compare, attest, re-attest, revoke) is owned by `integrity/` (§DF-05 OPT-B).
- Provenance is NOT legal evidence — evidence linkage belongs in `shared/provenance-evidence/evidence`.
