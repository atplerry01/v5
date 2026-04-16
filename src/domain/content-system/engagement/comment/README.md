# Comment Domain

**Path:** `content-system/engagement/comment`
**Namespace:** `Whycespace.Domain.ContentSystem.Engagement.Comment`

## Purpose
A single comment on a target resource (post, asset, thread). Owns body,
mentions, reply linkage, moderation flags, and redaction.

## Lifecycle
```
Posted ⇄ Edited
  │         │
  ├── Flag ─► Flagged (non-terminal; moderator review)
  └── Redact ─► Redacted (terminal)
```

## Events
- `CommentPostedEvent`
- `CommentEditedEvent`
- `CommentRedactedEvent`
- `CommentFlaggedEvent`
- `CommentRepliedEvent`

## Invariants
1. Author required.
2. Target reference required.
3. Body non-empty and bounded.
4. Redacted is terminal — no further edits.
