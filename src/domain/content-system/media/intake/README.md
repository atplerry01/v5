# media / intake

## Purpose

Groups **external → system ingress** aggregates for the media context. Distinct from the document `intake/` group which uses "upload" for user-driven ingress; media uses "ingest" per §CD-11 (pipeline-driven ingress with encoding jobs attached).

## Leaf domains

- `ingest/` — media ingest transaction (formerly `lifecycle/upload`; folder renamed per §CD-11). Aggregate class retains `MediaUpload` prefix pending CS.13 Band-F rename to `MediaIngest`.

## Boundary notes

- Ingest produces an asset + optionally a file; it does not own either.
- Handoff to processing happens via completion events; the ingest aggregate does not own processing state.
