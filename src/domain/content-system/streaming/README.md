# Streaming

## Purpose

The `streaming` context owns the truth of streaming delivery — the stream itself, its live-stream broadcast lifecycle, its channel, its per-viewer session, its delivery artifacts (manifest, segment, playback), its technical access grants, and its native observability (recording, metrics).

Everything in this context describes **how a stream technically exists and is delivered**, not who is watching, what they paid, or what programme is playing.

## Domain-groups

- `stream-core/` — the core streaming entities that constitute a stream's identity and session lifecycle: stream, live-stream, channel, stream-session.
- `delivery-artifact/` — artifacts produced to actually deliver a stream to a consumer: manifest, segment, playback descriptor.
- `control/` — technical control surfaces over a stream. Currently houses `access`, with room for further control domains (e.g. ingest control) as the context grows.
- `persistence-and-observability/` — stream-native durable outputs and measurements: recording, metrics.

## Ownership boundaries

### Owns

- Stream identity, mode, type, and lifecycle (Created / Active / Paused / Ended / Archived).
- Live-stream broadcast lifecycle (Created / Scheduled / Live / Paused / Ended / Cancelled) with broadcast window.
- Channel identity, stream binding, enable/disable/archive lifecycle.
- Stream-session lifecycle per viewer session (Opened / Active / Suspended / Closed / Failed / Expired) with window enforcement.
- Manifest lifecycle (Created / Published / Retired / Archived) with version progression.
- Segment lifecycle (Created / Published / Retired / Archived) with source reference and sequence number.
- Playback descriptor lifecycle (Created / Enabled / Disabled / Archived) with playback mode, window, and source reference.
- Technical access grant lifecycle (Granted / Restricted / Revoked / Expired) with mode, window, and token binding — purely technical, not commercial entitlement.
- Stream-native recording lifecycle (Started / Completed / Failed / Finalized / Archived).
- Stream-native metrics capture lifecycle (Capturing / Updated / Finalized / Archived) with bitrate, latency, drop, error, viewer, and playback counts.

### Does not own

- Commercial entitlement, subscription, payment — `control/access` models **technical** access, not whether a buyer is authorised.
- DRM policy decisions — policy engine layer.
- Programme / show / series metadata — that is an upstream commercial concern.
- Transcoder or CDN implementation — infrastructure adapters.
- Ad insertion or monetisation signalling.
- Viewer identity semantics — identity belongs to identity-system; `streaming` only holds opaque session and token-binding truth.

## Leaf domains

- `stream-core/stream` — root stream aggregate.
- `stream-core/live-stream` — broadcast lifecycle for live streaming.
- `stream-core/channel` — named channel bound to a stream.
- `stream-core/stream-session` — per-session lifecycle with window enforcement.
- `delivery-artifact/manifest` — streaming manifest (e.g. HLS/DASH-shaped) with version progression.
- `delivery-artifact/segment` — individual delivery segment.
- `delivery-artifact/playback` — playback descriptor controlling technical playback availability.
- `control/access` — technical stream-access grant.
- `persistence-and-observability/recording` — stream-native durable recording.
- `persistence-and-observability/metrics` — stream-native technical metrics.
