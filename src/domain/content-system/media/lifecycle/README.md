# media / lifecycle

## Purpose

Groups cross-cutting lifecycle aggregates that move media assets through ingestion, transformation, and version progression.

## Why this group exists

Upload, processing, and version are all **state-carrying transactions over a media artifact** — they are not media artifacts themselves, nor companions, nor descriptors. They share a semantic class (durable lifecycle aggregates) and belong together.

Retention is **not** currently modelled in media (it is only modelled under `document/lifecycle/retention`). If retention becomes applicable to media assets, it would be added to this group.

## Leaf domains

- `upload/` — media upload transaction (Requested → Accepted → Processing → Completed / Failed / Cancelled).
- `processing/` — media processing job (Requested → Running → Completed / Failed / Cancelled) with typed processing kind (transcode, thumbnail, normalise, package).
- `version/` — media version lineage (Draft → Active → Superseded / Withdrawn) with previous/successor linkage and a required `MediaFileRef`.

## Boundary notes

- Lifecycle aggregates reference media artifacts through local `*Ref` value objects (`MediaAssetRef`, `MediaFileRef`, `MediaProcessingInputRef`, `MediaUploadSourceRef`). They do not own the referenced artifact.
- Physical execution (transcoder, storage pipeline) lives in engines and infrastructure; these aggregates own transaction state and the event log.
- `media/lifecycle/version` always requires a concrete `MediaFileRef` — a media version is always bound to a specific backing file, unlike document versions which use a more general `ArtifactRef`.
