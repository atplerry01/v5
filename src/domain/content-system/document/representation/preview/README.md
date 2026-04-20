# preview (SCAFFOLD — pending implementation)

## Purpose

The `preview` leaf owns the **preview rendering lifecycle** — a durable domain truth about whether a preview for a given document version exists, its output ref, and its availability.

## Owns

- Preview identity, target ref (DocumentRef / DocumentVersionRef), preview output ref, status (Requested / Rendering / Available / Stale / Invalidated), render timestamps.
- Request / render / publish / invalidate / refresh transitions.

## Does not own

- The rendering engine or pipeline — infrastructure.
- Projections / read-model API shapes — belong in `projections/` and `platform/`.

## Status

SCAFFOLD only in P2.6.CS.3.
