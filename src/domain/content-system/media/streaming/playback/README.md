# Playback Domain

**Path:** `content-system/media/streaming/playback`
**Namespace:** `Whycespace.Domain.ContentSystem.Media.Streaming.Playback`

## Purpose
Owns a single viewer's playback session against a media asset: start,
pause, resume, complete, or stop.

## Lifecycle
```
Started ⇄ Paused ⇄ Resumed
   │         │        │
   └── Complete / Stop ──► terminal
```

## Events
- `PlaybackStartedEvent`
- `PlaybackPausedEvent`
- `PlaybackResumedEvent`
- `PlaybackCompletedEvent`
- `PlaybackStoppedEvent`

## Invariants
1. Asset and viewer references required.
2. Position ≥ 0.
3. Completed/Stopped are terminal.
