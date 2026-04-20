# Document

## Purpose

The `document` context owns the truth of stored document artifacts — their identity, their backing files, their structured groupings, their descriptor metadata, and their document-centric lifecycle (upload, processing, retention, version).

A "document" here is a content object — not a legal instrument, not an approval workflow, not an entitlement. The context models what the document **is** and how its bytes and structure evolve, nothing about what business meaning is attached downstream.

## Domain-groups

- `core-object/` — the document object itself and its immediate structural neighbours: the stored file, named groupings (bundle), audit-closure containers (record), and reusable templates.
- `descriptor/` — descriptive metadata attached to a document. Typed key/value entries with finalize semantics.
- `intake/` — external → system ingress (upload, import).
- `lifecycle-change/` — internal state transitions (processing — first-class per §CD-02a; version; review; publication).
- `governance/` — compliance/policy aggregates (retention, classification, moderation).
- `integrity-provenance/` — (SCAFFOLD pending CS.3) attestations and provenance.
- `representation/` — (SCAFFOLD pending CS.3) derived renderings (preview, export).

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

- `core-object/document` — the document aggregate.
- `core-object/file` — stored file bytes truth.
- `core-object/bundle` — named grouping of document members.
- `core-object/record` — lockable/closable document record container.
- `core-object/template` — reusable document template.
- `descriptor/metadata` — descriptive metadata entries attached to a document.
- `intake/upload` — document upload transaction.
- `lifecycle-change/processing` — document processing job. First-class per §CD-02a.
- `lifecycle-change/version` — document version lineage.
- `governance/retention` — retention attachment and lifecycle.
