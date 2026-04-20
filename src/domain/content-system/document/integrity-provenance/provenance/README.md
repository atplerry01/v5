# provenance (SCAFFOLD — pending implementation)

## Purpose

The `provenance` leaf owns the **chain of custody** and origin claims for a document — from source system through any transformation points to current state. Each claim is a provenance-record attached to the document.

## Owns

- Provenance record identity, target ref (DocumentRef), source claim (system, batch, operator, timestamp), chain position, status.
- Claim / counter-claim / supersede transitions.

## Does not own

- Legal evidence semantics — belongs to `shared/provenance-evidence/evidence`.
- The underlying transformations — those are `lifecycle-change/processing` events.
- Authorial / intellectual-property claims — those are upstream rights-management concerns.

## Status

SCAFFOLD only in P2.6.CS.3.
