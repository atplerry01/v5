# streaming / delivery-governance

## Purpose

Groups **delivery governance** concerns — access grants, entitlement hooks to upstream systems, moderation, and observability.

## Leaf domains

- `access/` — technical stream-access grant (Granted / Restricted / Revoked / Expired) with mode, window, and token binding. Moved from `control/access` per §CD-09.
- `observability/` — streaming observability capture (bitrate, latency, drop, error counts). Moved from `persistence-and-observability/metrics` per §DF-02 closure. Aggregate class `MetricsAggregate` retains its name pending CS.13 Band-F rename.
- `entitlement-hook/` — (SCAFFOLD pending CS.7) adapter to an upstream entitlement-of-record system (commerce/rights). Distinct from `access` per §CD-09.
- `moderation/` — (SCAFFOLD pending CS.7) streaming-level moderation decisions.

## Boundary notes

- `access` models TECHNICAL access only (window, token, mode). Commercial entitlement lives upstream — the `entitlement-hook/` scaffold is the adapter.
- `observability` is streaming-local metrics/telemetry. Platform-wide observability belongs in a different system.
- Moderation decisions on content (documents, media) live in their own context governance, not here.
