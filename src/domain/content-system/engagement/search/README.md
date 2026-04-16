# Search Domain

**Path:** `content-system/engagement/search`
**Namespace:** `Whycespace.Domain.ContentSystem.Engagement.Search`

## Purpose
Owns a named search index holding normalised document references.
`QueryNormalizationService` encapsulates pure text normalisation and
tokenisation.

## Lifecycle
```
Open ── Compact ──► Compacted (terminal)
  │
  ├── IndexDocument / Purge
```

## Events
- `SearchIndexCreatedEvent`
- `SearchDocumentIndexedEvent`
- `SearchDocumentPurgedEvent`
- `SearchIndexCompactedEvent`

## Invariants
1. Name required.
2. Document refs unique in index.
3. Compacted is terminal — no mutations.
