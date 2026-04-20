# Document

## Purpose

The `document` context owns the truth of stored document artifacts — their identity, their backing files, their structured groupings, their descriptor metadata, and their document-centric lifecycle (upload, processing, retention, version).

A "document" here is a content object — not a legal instrument, not an approval workflow, not an entitlement. The context models what the document **is** and how its bytes and structure evolve, nothing about what business meaning is attached downstream.

## Domain-groups

- `content-artifact/` — the document object itself and its immediate structural neighbours: the stored file, named groupings (bundle), audit-closure containers (record), and reusable templates.
- `descriptor/` — descriptive metadata attached to a document. Typed key/value entries with finalize semantics.
- `lifecycle/` — cross-cutting lifecycle aggregates that move documents through ingestion, transformation, retention, and version progression.

## Ownership boundaries

### Owns

- Document identity, title, type, classification, status (Draft / Active / Archived).
- Document-file byte truth: storage reference, declared and computed checksum, MIME, size, integrity and registration state.
- Bundle: a named grouping of document-centric members with finalize/archive semantics.
- Record: a document-closure container with lock/unlock/close/archive semantics.
- Template: typed document template with draft/active/deprecated/archived lifecycle.
- Descriptor metadata: typed key/value entries on a document, with add / update / remove / finalize.
- Upload transaction truth for documents.
- Processing job truth for documents (OCR, extraction, redaction, conversion, render).
- Retention attachment and retention lifecycle applied to document-scoped targets.
- Version lineage for documents, with previous/successor linkage and supersession rules.

### Does not own

- Document workflow meaning (approval, signing, review, notarisation) — belongs to workflow/orchestration layers.
- Legal evidence semantics — belongs to upstream legal/compliance domains.
- Access entitlement / permission decisions — belongs to policy and identity layers.
- Document rendering pipelines or OCR engine implementations — those are infrastructure adapters driven from these domain events.

## Leaf domains

- `content-artifact/document` — the document aggregate.
- `content-artifact/file` — stored file bytes truth.
- `content-artifact/bundle` — named grouping of document members.
- `content-artifact/record` — lockable/closable document record container.
- `content-artifact/template` — reusable document template.
- `descriptor/metadata` — descriptive metadata entries attached to a document.
- `lifecycle/upload` — document upload transaction.
- `lifecycle/processing` — document processing job.
- `lifecycle/retention` — retention attachment and lifecycle.
- `lifecycle/version` — document version lineage.
