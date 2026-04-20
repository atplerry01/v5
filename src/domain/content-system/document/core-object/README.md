# document / core-object

## Purpose

Groups the document object itself and its immediate structural neighbours — the stored file, named groupings (bundle), audit-closure containers (record), and reusable templates. `core-object` is the canonical home for durable document-truth aggregates within the `document` context.

## Why this group exists

A document doesn't live alone. It is usually paired with a concrete file, occasionally grouped into a named bundle, sometimes wrapped by a record that controls closure and locking, and sometimes produced from a template. These domains share the same semantic class — they are the **core objects** of the document context: primary durable-truth aggregates. Grouping them separates them from descriptor (metadata), intake (upload/import), lifecycle-change (processing/version/review/publication), integrity-provenance, representation, and governance concerns.

## Leaf domains

- `bundle/` — named grouping of document members with finalize/archive semantics.
- `document/` — the document aggregate root (identity, title, type, classification, status).
- `file/` — stored file bytes with integrity verification and supersession.
- `record/` — lockable/closable document-record container.
- `template/` — reusable document template with draft/active/deprecated/archived lifecycle.
- `attachment/` — (SCAFFOLD pending CS.3) association of a file to a parent record/document with its own lifecycle.

## Boundary notes

- Descriptor metadata belongs in `descriptor/metadata`, not here.
- Intake, lifecycle-change, integrity-provenance, representation, and governance aggregates live in their own groups under `document/`.
- Local ref value-objects (`DocumentRef`, `BundleMemberRef`) stay domain-local.
