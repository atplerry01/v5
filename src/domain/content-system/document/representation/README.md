# document / representation

## Purpose

Groups **derived renderings** of a document — previews and exports — each with their own lifecycle (render job, cache, invalidation, availability).

## Why this group exists

Representations are derived from the document but have their own durable lifecycle distinct from the document's own state. They are not `core-object/` (they are derived, not primary), not `lifecycle-change/` (they do not transition the document itself), and not projections (they are domain-level truth about what is available to render, not a read model).

## Leaf domains

- `preview/` — (SCAFFOLD) preview rendering aggregate. Tracks preview-ref, render lifecycle, availability, invalidation.
- `export/` — (SCAFFOLD) export rendering aggregate. Tracks export-ref, format, render lifecycle, availability, invalidation.

## Boundary notes

- Actual rendering pipelines are infrastructure — the aggregate owns only the rendering-job truth, not the engine.
- Distinct from `projections/` read models: representation aggregates own DOMAIN truth about whether a render exists, not the read-side materialisation.
