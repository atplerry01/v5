# Media Metadata Domain

**Path:** `content-system/media/metadata`
**Namespace:** `Whycespace.Domain.ContentSystem.Media.Metadata`

## Purpose
Holds descriptive key/value fields and tags for a media asset. Separate
from the asset aggregate so metadata can evolve independently and be
locked without touching asset bytes.

## Lifecycle
```
Attached ── Lock ──► Locked (terminal)
   │
   └── SetField / AddTag (repeated)
```

## Events
- `MetadataAttachedEvent`
- `MetadataFieldUpdatedEvent`
- `MetadataTaggedEvent`
- `MetadataLockedEvent`

## Invariants
1. Must reference an asset.
2. Locked metadata is immutable — no further fields or tags.
3. Tag uniqueness (normalized, case-insensitive).
