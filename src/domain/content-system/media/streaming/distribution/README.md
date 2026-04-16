# Distribution Policy Domain

**Path:** `content-system/media/streaming/distribution`
**Namespace:** `Whycespace.Domain.ContentSystem.Media.Streaming.Distribution`

## Purpose
Owns the set of distribution channels attached to a media asset
(e.g. CDN edges, regions, protocols) and whether the policy is active.

## Lifecycle
```
Active ── Deactivate ──► Deactivated (terminal)
  │
  └── AddChannel / RemoveChannel
```

## Events
- `DistributionPolicyAttachedEvent`
- `DistributionChannelAddedEvent`
- `DistributionChannelRemovedEvent`
- `DistributionPolicyDeactivatedEvent`

## Invariants
1. Must reference an asset.
2. Channel names are unique (normalized).
3. Deactivated policies are immutable.
