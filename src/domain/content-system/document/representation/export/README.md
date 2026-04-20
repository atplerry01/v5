# export (SCAFFOLD — pending implementation)

## Purpose

The `export` leaf owns the **export rendering lifecycle** — a durable domain truth about whether an export (PDF/DOCX/XML/etc.) for a given document version exists, its output ref, its format, and its availability.

## Owns

- Export identity, target ref (DocumentRef / DocumentVersionRef), export format, export output ref, status (Requested / Rendering / Available / Stale / Invalidated).
- Request / render / publish / invalidate / refresh transitions.

## Does not own

- The rendering engine or export generator — infrastructure.
- Projections / read-model API shapes.

## Status

SCAFFOLD only in P2.6.CS.3.
