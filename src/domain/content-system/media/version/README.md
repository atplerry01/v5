# Media Version Domain

**Path:** `content-system/media/version`
**Namespace:** `Whycespace.Domain.ContentSystem.Media.Version`

## Purpose
Tracks an individual version (semantic `Major.Minor.Patch`) of a media
asset through draft, promotion, supersession, and retirement.

## Lifecycle
```
Draft ── Promote ──► Promoted ── Supersede ──► Superseded
   │                    │                        │
   └── Retire ──► Retired (terminal from any)
```

## Events
- `AssetVersionDraftedEvent`
- `AssetVersionPromotedEvent`
- `AssetVersionSupersededEvent`
- `AssetVersionRetiredEvent`

## Invariants
1. Must reference a media asset.
2. Version components are non-negative.
3. Only Draft may be promoted.
4. Only Promoted may be superseded.
5. Retired is terminal.
