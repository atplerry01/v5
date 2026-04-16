# Reaction Domain

**Path:** `content-system/engagement/reaction`
**Namespace:** `Whycespace.Domain.ContentSystem.Engagement.Reaction`

## Purpose
A single actor's reaction (Like, Love, Laugh, Insightful, Celebrate, Sad)
on a target content item.

## Lifecycle
```
Added ⇄ Changed ── Remove ──► Removed (terminal)
```

## Events
- `ReactionAddedEvent`
- `ReactionChangedEvent`
- `ReactionRemovedEvent`

## Invariants
1. Actor required.
2. Target required.
3. Kind cannot change to same value.
4. Removed is terminal.
