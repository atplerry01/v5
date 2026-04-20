# relationship (SCAFFOLD — pending implementation)

## Purpose

The `relationship` leaf owns **cross-context graph edges** between content objects: document-cites-media, media-part-of-stream, stream-archives-broadcast, etc. Each edge is a typed aggregate with its own lifecycle (established / superseded / revoked).

## Owns

- Relationship identity, source ref (opaque), target ref (opaque), relationship kind, status (Established / Superseded / Revoked).
- Establish / supersede / revoke transitions.
- Bi-directional attribution metadata (who asserted the edge, when, why).

## Does not own

- The source or target objects — those are owned by their respective contexts.
- Rendering of the graph — projection/read-model concern.
- Graph-traversal algorithms — read-side / query concern.

## Status

SCAFFOLD only in P2.6.CS.5.0.
