# document / content-artifact

## Purpose

Groups the document object itself and its immediate structural neighbours — the stored file, named groupings (bundle), audit-closure containers (record), and reusable templates.

## Why this group exists

A document doesn't live alone. It is usually paired with a concrete file, occasionally grouped into a named bundle, sometimes wrapped by a record that controls closure and locking, and sometimes produced from a template. These five domains share the same semantic class — they are all **content artifacts**: primary structural objects of the document context. Grouping them separates them from descriptor (metadata) and lifecycle (upload/processing/retention/version) concerns, which are semantically different classes.

## Leaf domains

- `document/` — the document aggregate root (identity, title, type, classification, status).
- `file/` — stored file bytes with integrity verification and supersession.
- `bundle/` — named grouping of document members with finalize/archive semantics.
- `record/` — lockable/closable document-record container.
- `template/` — reusable document template with draft/active/deprecated/archived lifecycle.

## Boundary notes

- Descriptor metadata belongs in `descriptor/metadata`, not here — metadata is a descriptive class, not a content artifact.
- Upload, processing, retention, and version aggregates belong in `lifecycle/`, not here — those are cross-cutting lifecycle concerns that attach to content artifacts but are not themselves content artifacts.
- A `DocumentFile` carries `DocumentRef`; a `DocumentRecord` carries `DocumentRef`; a `DocumentBundle` carries `BundleMemberRef` (which in turn can reference documents). These are local reference value objects, not shared-kernel types.
