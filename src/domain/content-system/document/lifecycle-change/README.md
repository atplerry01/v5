# document / lifecycle-change

## Purpose

Groups **internal state transitions** applied to a document after it is owned by the system — processing jobs, version progression, review gates, and publication lifecycle.

## Why this group exists

State transitions that operate on an already-owned document are distinct from ingress (`intake/`) and from governance/compliance concerns (`governance/`). They share the semantic class of "durable lifecycle transactions whose state represents controlled progress of a transition applied to a document."

## Leaf domains

- `processing/` — document processing job (OCR, extraction, redaction, conversion, render). **First-class per §CD-02a — LOCKED.**
- `version/` — document version lineage with previous/successor linkage.
- `review/` — (SCAFFOLD pending CS.3) pre-publication review gate.
- `publication/` — (SCAFFOLD pending CS.3) publication lifecycle distinct from version activation.

## Boundary notes

- These aggregates reference documents/files via local `*Ref` value objects; they do not own the referenced core object.
- Retention and classification are **not** here — those are `governance/` concerns.
- Physical execution (OCR engine, render pipeline) lives in infrastructure adapters driven from these domain events.
