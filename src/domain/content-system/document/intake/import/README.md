# import (SCAFFOLD — pending implementation)

## Purpose

The `import` leaf owns **batch/programmatic ingress** from external source systems — distinct from synchronous user-driven `upload/`. An import job references a source system, an optional batch identifier, and tracks batch-level completion.

## Owns

- Batch import transaction truth: import-job identity, source-system ref, batch ref, status (Requested / Running / Completed / Failed / Cancelled), per-item progress counters.
- Request / start / report-progress / complete / fail / cancel transitions.
- Source-system attribution and migration provenance at ingest time.

## Does not own

- The documents or files produced by the import — owned by `document/core-object/document` and `document/core-object/file`.
- Source-system connectors, migration scripts, or batch schedulers — infrastructure.
- Per-document transformation — delegated to `lifecycle-change/processing`.

## Status

SCAFFOLD only in P2.6.CS.3. Implementation deferred to a feature phase. The 7 mandatory artifact subfolders exist; contents are `.gitkeep` placeholders.
