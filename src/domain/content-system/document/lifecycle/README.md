# document / lifecycle

## Purpose

Groups cross-cutting lifecycle aggregates that move documents (and their files and versions) through ingestion, transformation, retention, and version progression.

## Why this group exists

Upload, processing, retention, and version are all **state-carrying transactions over a content artifact** — they are not content artifacts themselves, and they are not descriptors. They share a semantic class — durable lifecycle aggregates whose state represents the progress of a controlled transition applied to a document.

Grouping them into `lifecycle/` separates them from `content-artifact/` (primary structural objects) and `descriptor/` (descriptive metadata), and leaves room for future lifecycle aggregates (e.g. destruction, legal-hold-release) without restructuring.

## Leaf domains

- `upload/` — document upload transaction (Requested → Accepted → Processing → Completed / Failed / Cancelled).
- `processing/` — document processing job (Requested → Running → Completed / Failed / Cancelled) with typed processing kind (OCR, extraction, redaction, conversion, render).
- `retention/` — retention attachment and retention lifecycle (Applied / Held / Released / Expired / EligibleForDestruction / Archived).
- `version/` — document version lineage (Draft → Active → Superseded / Withdrawn) with previous/successor linkage.

## Boundary notes

- Lifecycle aggregates reference content artifacts through local `*Ref` value objects (e.g. `DocumentRef`, `ProcessingInputRef`, `RetentionTargetRef`, `ArtifactRef`). They do not own the referenced artifact.
- Physical execution of an upload, processing job, or retention policy lives in the engine / runtime / infrastructure layers — these aggregates own only the transaction state and event log.
- Retention does **not** perform destruction. It models when a target becomes eligible for destruction; the actual destruction is orchestrated downstream.
