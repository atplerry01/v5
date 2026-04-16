# Messaging Domain

**Path:** `content-system/interaction/messaging`
**Namespace:** `Whycespace.Domain.ContentSystem.Interaction.Messaging`

## Purpose
Owns the lifecycle of a single message exchanged inside a conversation:
send, deliver, read, edit, retract.

## Aggregate Lifecycle
```
Draft → Sent → Delivered → Read
  │        │        │
  └────────┴────────┴── Retracted (terminal)
```

## Events
- `MessageSentEvent`
- `MessageDeliveredEvent`
- `MessageReadEvent`
- `MessageEditedEvent`
- `MessageRetractedEvent`

## Invariants
1. A sent message must have a non-empty sender.
2. A sent message must belong to a conversation.
3. Message body is non-empty and bounded.
4. Read messages cannot be edited.
5. Retracted messages cannot transition further.
6. Attachment IDs are unique per message.
