# document / descriptor

## Purpose

Groups descriptive metadata attached to a document. Descriptor aggregates hold typed key/value entries that describe a document without being the document itself.

## Why this group exists

Metadata is a distinct semantic class from core objects and lifecycle aggregates: it describes the object but is neither the object nor a lifecycle transition over it. Isolating it into its own domain-group keeps that separation clean and leaves room for additional descriptor-class domains (e.g. tagging, classification-tree attachment) without polluting `core-object/` or `lifecycle/`.

## Leaf domains

- `metadata/` — typed key/value metadata entries attached to a document, with add / update / remove / finalize.

## Boundary notes

- Does not own the document itself — only its descriptor entries. Holds a `DocumentRef` back-pointer.
- Does not own access policy, audit logging, or search indexing — those are downstream concerns.
- Once finalized, metadata is immutable (enforced by the aggregate, not by infrastructure).
