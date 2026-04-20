# document / intake

## Purpose

Groups **external → system ingress** aggregates for the document context. Intake domains own the transaction truth of bringing a document (or batch of documents) from outside the system into document-context ownership.

## Why this group exists

Ingress is a distinct semantic class from internal state transitions (`lifecycle-change/`) and from governance (`governance/`). A user-driven upload and a batch import both end when the document becomes owned by the system; everything that happens to the document AFTER that is a different class of transition.

## Leaf domains

- `upload/` — synchronous, user-driven ingress transaction (Requested → Accepted → Processing → Completed/Failed/Cancelled).
- `import/` — (SCAFFOLD pending CS.3) batch/programmatic ingress from external source systems.

## Boundary notes

- Intake aggregates reference produced artifacts via opaque `*OutputRef` value objects. They do not own the produced document/file.
- Handoff to processing: today, `DocumentUploadProcessingStartedEvent` lives on `upload`. Boundary cleanup is tracked as §DF-07 (non-blocking for realignment).
