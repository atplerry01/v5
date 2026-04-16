# Feed Domain

**Path:** `content-system/engagement/feed`
**Namespace:** `Whycespace.Domain.ContentSystem.Engagement.Feed`

## Purpose
Owner-scoped ranked collection of feed items. Owns append, pin, and
clear operations. Ordering logic lives in the pure domain service
`FeedOrderingService`.

## Events
- `FeedCreatedEvent`
- `FeedItemAppendedEvent`
- `FeedItemPinnedEvent`
- `FeedClearedEvent`

## Invariants
1. Owner required.
2. Item refs unique per feed.
3. Rank non-negative.
4. Pin requires the item to exist.

## Service
`FeedOrderingService` — pure ordering (rank desc → recency desc → ref).
