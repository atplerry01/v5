# Presence Domain

**Path:** `content-system/interaction/presence`
**Namespace:** `Whycespace.Domain.ContentSystem.Interaction.Presence`

## Purpose
Tracks an actor's availability signal: register, change status, heartbeat, expire.

## Lifecycle
```
Registered(Online|Away|Busy) ──Heartbeat──► self
    │
    ├─ ChangeStatus → Online/Away/Busy/Offline
    └─ Expire ─► Expired (terminal)
```

## Events
- `PresenceRegisteredEvent`
- `PresenceStatusChangedEvent`
- `PresenceHeartbeatRecordedEvent`
- `PresenceExpiredEvent`

## Invariants
1. Actor reference required once registered.
2. No transitions out of Expired.
3. Heartbeat forbidden after expiry.
