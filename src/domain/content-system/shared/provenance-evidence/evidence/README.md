# evidence (SCAFFOLD — pending implementation)

## Purpose

The `evidence` leaf owns **cross-context evidence linkage** — binding a content object (document / media / stream archive) to an evidence chain for legal, forensic, or audit purposes. An evidence aggregate tracks attestations, chain-of-custody across context boundaries, and evidentiary status.

## Owns

- Evidence identity, bound-target ref (opaque), attestation chain, evidentiary status (Collected / Attested / Sealed / Broken), custodians.
- Collect / attest / seal / break / transfer transitions.

## Does not own

- Document-local custody — that is `document/integrity-provenance/provenance`.
- Legal semantics outside the evidence chain — upstream compliance/legal domain.
- Cryptographic signing implementation — infrastructure.

## Boundary notes

Distinct from `document/integrity-provenance/provenance`:
- Document provenance = **chain of custody for the document's own bytes and transformations** (document-local).
- Shared evidence = **cross-context evidentiary binding** (this document + this media rendition + this stream archive are all evidence items in the same case).

## Status

SCAFFOLD only in P2.6.CS.5.0.
