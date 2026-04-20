# ingest-session (SCAFFOLD — pending implementation)

## Purpose

The `ingest-session` leaf owns **live-broadcast ingest session** — the upstream ingress of a broadcaster's feed into the system, distinct from the `broadcast` lifecycle (which tracks downstream viewer-facing state).

## Owns (planned)

- Ingest-session identity, broadcast ref, source endpoint, status (Authenticated / Streaming / Stalled / Ended / Failed), ingest timestamps.
- Authenticate / start-streaming / stall / resume / end / fail transitions.

## Does not own

- The broadcast lifecycle itself — owned by `live-streaming/broadcast`.
- Transcoder or RTMP/SRT implementation — infrastructure.
- Broadcaster identity — identity system.

## Status

SCAFFOLD only in P2.6.CS.7.
