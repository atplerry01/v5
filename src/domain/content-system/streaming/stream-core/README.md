# streaming / stream-core

## Purpose

Groups the **core streaming entities** that constitute a stream's identity and descriptor set: the stream itself, its named channel, its manifest (playlist/MPD), and its availability descriptor.

## Why this group exists

These are the four authoritative descriptors of a stream — its identity and the immediately attached structural facts that define what the stream IS. Viewer-side consumption (session / progress / replay) lives in `playback-consumption/`. Broadcast lifecycle (scheduling / live ramp) lives in `live-streaming/`. Governance (access / moderation / observability) lives in `delivery-governance/`.

## Leaf domains

- `stream/` — root stream aggregate (mode, type, Created / Active / Paused / Ended / Archived).
- `channel/` — named channel bound to a stream (Created / Enabled / Disabled / Archived).
- `manifest/` — streaming manifest (HLS playlist / DASH MPD) with version progression (moved from `delivery-artifact/` per §CD-05).
- `availability/` — availability descriptor for playback (source + mode + window + enabled/disabled) — moved from `delivery-artifact/playback` per §DF-01 verdict. Aggregate class names retain `Playback` prefix pending CS.13 Band-F rename.

## Boundary notes

- `live-stream` has MOVED to `live-streaming/broadcast` — not in this group any more.
- `stream-session` has MOVED to `playback-consumption/session` — not in this group any more.
- Delivery-artifact/segment has been RETIRED per §CD-07 verdict (§claude/new-rules/20260420-091444-audits.md).
