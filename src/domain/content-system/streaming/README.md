# Streaming

## Purpose

The `streaming` context owns the truth of streaming delivery — stream identity, manifest, availability descriptors, channels, playback consumption (viewer sessions), live broadcast, archive, and delivery governance (access, entitlement hooks, moderation, observability).

Everything here describes **how a stream technically exists and is delivered**, not who is watching commercially, what they paid, or what programme is playing.

## Domain-groups

- `stream-core/` — core streaming entities that constitute a stream's identity: `stream`, `channel`, `manifest`, `availability`.
- `playback-consumption/` — viewer-side session and progress concerns: `session` (and scaffolded `progress`, `replay` in CS.7).
- `live-streaming/` — live broadcast concerns: `broadcast` (moved from stream-core/live-stream), with scaffolded `ingest-session` and live `archive` (CS.6 and CS.7).
- `delivery-governance/` — access, entitlement hooks, moderation, observability: `access`, `observability` (moved from metrics), plus scaffolded `entitlement-hook` and `moderation` in CS.7.

**Removed in P2.6.CS.5:** `delivery-artifact/` (manifest moved to stream-core; segment retired; playback moved to stream-core/availability). `control/` (access moved). `persistence-and-observability/` (metrics moved to delivery-governance/observability; recording splits in CS.6).

## Ownership boundaries

### Owns

- Stream identity, mode, type, and lifecycle (Created / Active / Paused / Ended / Archived).
- Broadcast lifecycle (Created / Scheduled / Live / Paused / Ended / Cancelled) with broadcast window. (Currently held by `live-streaming/broadcast/LiveStreamAggregate`; class-level rename deferred to CS.13 per CS.5 discipline capture.)
- Channel identity, stream binding, enable/disable/archive lifecycle.
- Session lifecycle per viewer session (Opened / Active / Suspended / Closed / Failed / Expired) with window enforcement. (Held by `playback-consumption/session/StreamSessionAggregate` — rename deferred.)
- Manifest lifecycle (Created / Published / Retired / Archived) with version progression.
- Availability descriptor lifecycle (Created / Enabled / Disabled / Archived) with playback mode, window, and source reference. (Held by `stream-core/availability/PlaybackAggregate` — rename deferred per §DF-01 verdict.)
- Technical access grant lifecycle (Granted / Restricted / Revoked / Expired) with mode, window, and token binding — purely technical, not commercial entitlement.
- Stream-native recording lifecycle (Started / Completed / Failed / Finalized / Archived). (Will SPLIT in CS.6 into `live-streaming/archive` + `playback-consumption/replay`.)
- Observability capture lifecycle with bitrate, latency, drop, error, viewer, and playback counts. (Held by `delivery-governance/observability/MetricsAggregate` — rename deferred.)

### Does not own

- Commercial entitlement, subscription, payment — `delivery-governance/access` models **technical** access only; commercial entitlement is upstream.
- DRM policy decisions — policy engine layer.
- Programme / show / series metadata — that is an upstream commercial concern.
- Transcoder or CDN implementation — infrastructure adapters.
- Ad insertion or monetisation signalling.
- Viewer identity semantics — identity belongs to identity-system; `streaming` only holds opaque session and token-binding truth.
- Per-segment delivery truth — `segment` RETIRED in CS.5 per §CD-07 verdict (§claude/new-rules/20260420-091444-audits.md).

## Leaf domains (post CS.5)

- `stream-core/stream` — root stream aggregate.
- `stream-core/channel` — named channel bound to a stream.
- `stream-core/manifest` — streaming manifest with version progression.
- `stream-core/availability` — availability descriptor (formerly `delivery-artifact/playback`; maps to target canonical `availability`).
- `playback-consumption/session` — per-session viewer attachment (formerly `stream-core/stream-session`).
- `live-streaming/broadcast` — live broadcast lifecycle (formerly `stream-core/live-stream`).
- `delivery-governance/access` — technical stream-access grant (formerly `control/access`).
- `delivery-governance/observability` — metrics/observability (formerly `persistence-and-observability/metrics`).
- `live-streaming/archive` — stream-native recording/archive (moved from legacy `persistence-and-observability/recording` in CS.6; events broadcast-side only per §DF-06).
- `playback-consumption/replay` — (SCAFFOLD) viewer-initiated replay of an archive; no events migrated from legacy recording.

## E1→Ex delivery status (2026-04-21)

Full vertical slice (sections 1–15, 18) delivered for the **9 populated BCs** per [claude/templates/delivery-pattern/](../../../../claude/templates/delivery-pattern/):

| BC | Events | T2E handlers | Projection | Controller | Topics | Policy | Schema registration |
|---|---|---|---|---|---|---|---|
| stream-core/stream | 6 | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| stream-core/channel | 5 | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| stream-core/manifest | 5 | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| stream-core/availability *(PlaybackAggregate)* | 5 | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| live-streaming/broadcast | 7 | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| live-streaming/archive | 5 | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| playback-consumption/session | 7 | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| delivery-governance/access *(StreamAccessAggregate)* | 5 | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
| delivery-governance/observability | 4 | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |

**Cross-system invariants** (per [01-domain-skeleton.md § Cross-System Invariants](../../../../claude/templates/delivery-pattern/01-domain-skeleton.md)) — see [../invariant/](../invariant/README.md):

- `BroadcastStreamBindingPolicy` — enforced in [CreateBroadcastHandler](../../../../engines/T2E/content/streaming/live-streaming/broadcast/CreateBroadcastHandler.cs) via [IStreamStatusLookup](../../../../shared/contracts/content/streaming/stream-core/stream/IStreamStatusLookup.cs).
- `SessionStreamAccessPolicy` — registered, enforcement deferred (session aggregate lacks subject/access-id binding).

**Still D0 (scaffold only):** `live-streaming/ingest-session`, `playback-consumption/progress`, `playback-consumption/replay`, `delivery-governance/entitlement-hook`, `delivery-governance/moderation`.

**Section 16 (Tests):** not yet delivered for streaming — follows the same gap as the media context. Tests are tracked as a separate content-system work item.
