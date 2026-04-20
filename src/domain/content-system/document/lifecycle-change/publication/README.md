# publication (SCAFFOLD — pending implementation)

## Purpose

The `publication` leaf owns the **publication lifecycle** for a document — distinct from version activation. A document may have an active version without yet being published; publication is a separate visibility/availability commitment.

## Owns

- Publication identity, target ref (DocumentRef / DocumentVersionRef), audience scope, status (Unpublished / Published / Withdrawn), publication timestamps.
- Publish / republish / withdraw transitions.

## Does not own

- Version lineage — owned by `lifecycle-change/version`.
- Access entitlement — policy/identity layer.
- Channel-specific rendering or distribution — infrastructure / downstream.

## Status

SCAFFOLD only in P2.6.CS.3.
