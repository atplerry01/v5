# Conversation Domain

**Path:** `content-system/interaction/conversation`
**Namespace:** `Whycespace.Domain.ContentSystem.Interaction.Conversation`

## Purpose
Groups messaging participants in a named thread. Owns participant
join/leave, topic rename, archival.

## Aggregate Lifecycle
```
Active ── Archive ──► Archived (terminal)
  │
  └── Join/Leave participants, Rename topic
```

## Events
- `ConversationStartedEvent`
- `ParticipantJoinedEvent`
- `ParticipantLeftEvent`
- `ConversationRenamedEvent`
- `ConversationArchivedEvent`

## Invariants
1. Active conversation has at least one active participant.
2. Cannot mutate an archived conversation.
3. Cannot double-join or leave when not a participant.
4. Topic is non-empty and bounded.
