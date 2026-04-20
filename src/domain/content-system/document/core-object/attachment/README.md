# attachment (SCAFFOLD — pending implementation)

## Purpose

The `attachment` leaf owns the **association** of a file to a parent document, record, or bundle, as its own first-class aggregate with its own lifecycle — distinct from the file itself and the parent object.

## Owns

- Attachment identity, parent ref (DocumentRef / RecordRef / BundleRef), attached file ref, relationship kind, status (Attached / Detached / Archived).
- Attach / detach / replace / archive transitions and the attachment-level audit trail.

## Does not own

- The attached file's bytes — owned by `document/core-object/file`.
- The parent document / record / bundle — owned by their respective leaves.
- Access policy for the attachment — policy/identity layer.

## Boundary notes

Today, bundles point at document IDs via `BundleMemberRef`. The attachment aggregate is the canonical home for the "this file is attached to THAT parent" relationship, allowing multi-kind attachments (file-to-record, file-to-document-outside-bundle, etc.) without overloading bundle semantics.

## Status

SCAFFOLD only in P2.6.CS.3. Implementation deferred to a feature phase.
