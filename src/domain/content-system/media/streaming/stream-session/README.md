# Stream Session Domain

**Path:** `content-system/media/streaming/stream-session`
**Namespace:** `Whycespace.Domain.ContentSystem.Media.Streaming.StreamSession`

## Purpose
Broadcast/multicast stream session, tracking open endpoint, viewer
join/leave, and closure or forced termination.

## Lifecycle
```
Open ── Close/Terminate ──► Closed / Terminated (terminal)
   │
   └── Viewer join/leave (repeatable while open)
```

## Events
- `StreamOpenedEvent`
- `StreamViewerJoinedEvent`
- `StreamViewerLeftEvent`
- `StreamClosedEvent`
- `StreamTerminatedEvent`

## Invariants
1. Asset reference required.
2. Viewer mutations only when Open.
3. Closed/Terminated are terminal.
