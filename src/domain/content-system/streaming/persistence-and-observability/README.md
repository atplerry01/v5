# streaming / persistence-and-observability

## Purpose

Groups stream-native durable outputs and measurements: recording (durable persisted output of a stream) and metrics (technical measurement of a stream's runtime behaviour).

## Why this group exists

Recording and metrics share the semantic class of **stream-native observable truth** — they are both produced by a running stream, owned by the `streaming` context, and distinct from delivery artifacts (which exist to deliver the stream) and control artifacts (which gate access to it). Grouping them together keeps that distinction explicit.

## Leaf domains

- `recording/` — durable recording of a stream (Started / Completed / Failed / Finalized / Archived), optionally bound to a `StreamSessionRef`.
- `metrics/` — captured technical metrics for a stream (Capturing / Updated / Finalized / Archived) with bitrate, latency, drop, error, viewer and playback counts.

## Boundary notes

- Recording and metrics both carry `StreamRef`. Metrics may additionally carry `RecordingRef` when measuring a specific recording.
- This is **technical** observability. Business KPIs, analytics dashboards, and finance-side metrics are downstream concerns and are not modelled here.
- The transcoder / writer pipeline that physically produces a recording is infrastructure. The domain aggregate owns only the recording's lifecycle state and output reference.
