# Call Domain

**Path:** `content-system/interaction/call`
**Namespace:** `Whycespace.Domain.ContentSystem.Interaction.Call`

## Purpose
Models a voice/video/screen call session from initiation through answer,
participant join/leave, and termination.

## Lifecycle
```
Initiated → Ringing → Answered → Ended
    │          │
    ├──Reject─►Rejected (terminal)
    └──End────►Ended (terminal)
```

## Events
- `CallInitiatedEvent`
- `CallAnsweredEvent`
- `CallRejectedEvent`
- `CallEndedEvent`
- `CallParticipantJoinedEvent`

## Invariants
1. Initiator reference required.
2. Ended/Rejected are terminal — no further transitions.
3. Participants can only join answered calls.
