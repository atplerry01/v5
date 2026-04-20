# streaming / stream-core

## Purpose

Groups the core streaming entities that constitute a stream's identity and session lifecycle: the stream itself, its live-stream broadcast lifecycle, its named channel, and its per-viewer session.

## Why this group exists

These four domains are the **structural core** of streaming: they define what a stream is, how a broadcast unfolds, how channels bind to streams, and how individual sessions attach to streams. Everything else in `streaming/` either produces delivery artifacts from a stream (`delivery-artifact/`), controls access to it (`control/`), or records / measures it (`persistence-and-observability/`).

## Leaf domains

- `stream/` — root stream aggregate (mode, type, Created / Active / Paused / Ended / Archived).
- `live-stream/` — broadcast lifecycle for a live stream, including schedule window (Created / Scheduled / Live / Paused / Ended / Cancelled).
- `channel/` — named channel bound to a stream (Created / Enabled / Disabled / Archived).
- `stream-session/` — per-session attachment to a stream with window enforcement (Opened / Active / Suspended / Closed / Failed / Expired).

## Boundary notes

- `live-stream` and `channel` both carry `StreamRef`; they describe behaviour **around** a stream, not identity of the stream itself.
- `stream-session` enforces that the session cannot be opened after its window has expired, and tracks a distinct terminal fan-out (Closed / Failed / Expired).
- Delivery concerns (manifest, segment, playback) are explicitly **not** in this group.
- Access control is in `control/`, not here.
