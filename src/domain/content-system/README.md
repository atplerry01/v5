# Content-System

## Purpose

`content-system` is the pure DDD classification that owns the **truth of content artifacts** — files, media, and streams — and their lifecycle state. It models what a content object is, how it came into existence, how its bytes are persisted and verified, how versions evolve, what metadata describes it, and how streaming delivery artifacts are produced and recorded.

It is a technical truth layer. It does not model commercial, customer, legal, or workflow semantics.

## Canonical contexts

- `document` — stored document artifacts, their files, versions, metadata, and document-centric lifecycle (upload, processing, retention).
- `media` — typed rich-media artifacts (asset, audio, video, image), their backing files, companion artifacts (subtitle, transcript), metadata, and media-centric lifecycle (upload, processing, version).
- `streaming` — streaming technical truth: stream, channel, session, live-stream, delivery artifacts (manifest, segment, playback), technical access, and native persistence/observability (recording, metrics).

## Topology

`content-system` is governed by the canonical grouped DS-R3 / DS-R3a topology:

```
src/domain/content-system/{context}/{domain-group}/{domain}/
```

Every context under `content-system` uses the **4-level form** — fully grouped, no flat domains (per DS-R3a per-context atomicity).

Domain-groups are **folder/namespace organising concepts only**. They do NOT participate in routing, policy keys, Kafka topic names, or `DomainRoute`. Per DS-R8, `DomainRoute` remains the three-tuple `(classification, context, domain)` — the `domain-group` segment is intentionally absent from the routing key so that policy IDs, Kafka topic names, projection schemas, and consumer-group names remain stable across grouping evolution.

Example: `content-system/media/content-artifact/asset/` routes as `DomainRoute("content", "media", "asset")`.

## Ownership boundaries

### Owns

- Existence and identity of content artifacts (document, file, asset, audio, video, image, media-file, bundle, record, template, stream, channel, live-stream, stream-session, manifest, segment, playback).
- Stored file/media bytes truth: storage reference, declared checksum, MIME type, size, integrity verification state.
- Upload / ingestion transaction truth (request → accept → process → complete/fail/cancel).
- Processing job truth (request → run → complete/fail/cancel) — OCR, extraction, transcoding, transformation, conversion, render.
- Version lineage (draft → active → superseded / withdrawn) for both documents and media assets.
- Descriptor metadata (typed key/value entries attached to a document or media asset).
- Retention attachment and retention lifecycle (applied, held, released, expired, eligible-for-destruction, archived).
- Streaming technical delivery artifacts: manifest lifecycle, segment lifecycle, playback descriptor lifecycle.
- Stream-native recording truth and stream-native metrics truth.
- Technical access grants to streams (mode, window, token binding, restrict/unrestrict/revoke/expire).

### Does not own

- Business workflow meaning — orchestration belongs to engines and the workflow/orchestration runtime.
- Commercial meaning — pricing, entitlement, subscription, billing.
- Customer / order / provider semantics.
- Legal authority outside content-artifact truth (document-as-contract, document-as-evidence — those are upstream semantic layers).
- Operational orchestration — long-running processes, sagas, compensation.
- Infrastructure adapters — storage drivers, transcoder implementations, CDN integrations.
- Policy-engine implementation — WHYCEPOLICY decisioning lives in its own layer.
- Projections, read models, and API shapes — those live in `projections/` and `platform/`.

## Canonical structure

```
src/domain/content-system/
├── document/
│   ├── content-artifact/
│   │   ├── document/
│   │   ├── file/
│   │   ├── bundle/
│   │   ├── record/
│   │   └── template/
│   ├── descriptor/
│   │   └── metadata/
│   └── lifecycle/
│       ├── upload/
│       ├── processing/
│       ├── retention/
│       └── version/
├── media/
│   ├── content-artifact/
│   │   ├── asset/
│   │   ├── audio/
│   │   ├── video/
│   │   ├── image/
│   │   └── media-file/
│   ├── companion-artifact/
│   │   ├── subtitle/
│   │   └── transcript/
│   ├── descriptor/
│   │   └── metadata/
│   └── lifecycle/
│       ├── upload/
│       ├── processing/
│       └── version/
└── streaming/
    ├── stream-core/
    │   ├── stream/
    │   ├── live-stream/
    │   ├── channel/
    │   └── stream-session/
    ├── delivery-artifact/
    │   ├── manifest/
    │   ├── segment/
    │   └── playback/
    ├── control/
    │   └── access/
    └── persistence-and-observability/
        ├── recording/
        └── metrics/
```

Each leaf domain carries the canonical 7 artifact subfolders:
`aggregate/`, `entity/`, `error/`, `event/`, `service/`, `specification/`, `value-object/`.

## Implementation status

**E1-complete at D2 across all 31 canonical leaves.**

Every leaf has:
- a pure event-sourced aggregate root,
- typed value objects and status enums,
- a domain error catalogue,
- domain events for every state change,
- at least one specification where invariants require it.

There are zero external dependencies beyond `Whycespace.Domain.SharedKernel.Primitives.Kernel`, and no infrastructure, projection, or runtime coupling.
