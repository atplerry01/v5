# Media Asset Domain

**Classification:** `content-system`
**Context:** `media`
**Domain:** `asset`
**Namespace:** `Whycespace.Domain.ContentSystem.Media.Asset`

## Purpose

The Media Asset domain owns the lifecycle of a single piece of media
content (image, video, audio, document) as it moves from initial
registration through processing, availability, publication, optional
unpublication, and archival. The aggregate is event-sourced, pure, and
fully deterministic — all identity, time, correlation, and causation
inputs are supplied by callers; the domain itself reads neither the
system clock nor any random source.

## Scope

This BC contains a single aggregate root — `MediaAssetAggregate`. It
carries descriptive metadata (title, description, tags), a cryptographic
content digest, a storage location reference, and the current status. It
does NOT own content bytes, storage infrastructure, rendering,
transcoding, distribution, access control, or any cross-BC coordination.
Those concerns belong to engines/runtime/platform layers.

## Aggregate Lifecycle

```
                       Register
                          │
                          ▼
                     ┌────────┐
                     │ Draft  │
                     └───┬────┘
                         │ StartProcessing
                         ▼
                   ┌────────────┐
                   │ Processing │
                   └─────┬──────┘
                         │ MarkAvailable
                         ▼
            ┌──────────────────────┐
            │      Available       │◄────────┐
            └─────┬────────────┬───┘         │
                  │            │ Publish     │
                  │            ▼             │
                  │     ┌───────────┐        │
                  │     │ Published │────────┘ Unpublish
                  │     └─────┬─────┘
                  │           │
                  │ Archive   │ Archive
                  ▼           ▼
              ┌──────────────────┐
              │     Archived     │  (terminal)
              └──────────────────┘
```

Allowed transitions are enforced by
`MediaAssetTransitionSpecification`:

| From       | To                     |
|------------|------------------------|
| Draft      | Processing, Archived   |
| Processing | Available, Archived    |
| Available  | Published, Archived    |
| Published  | Available, Archived    |
| Archived   | — (terminal)           |

## Event Flow

Every state mutation flows through one of the following past-tense
domain events. Each event carries deterministic `EventId`,
`AggregateId`, `CorrelationId`, and `CausationId` (injected by callers)
plus a `Timestamp` supplied from an `IClock` at the runtime boundary.

| Command           | Event                              |
|-------------------|------------------------------------|
| `Register`        | `MediaAssetRegisteredEvent`        |
| `StartProcessing` | `MediaAssetProcessingStartedEvent` |
| `MarkAvailable`   | `MediaAssetAvailableEvent`         |
| `Publish`         | `MediaAssetPublishedEvent`         |
| `Unpublish`       | `MediaAssetUnpublishedEvent`       |
| `Archive`         | `MediaAssetArchivedEvent`          |
| `UpdateMetadata`  | `MediaAssetMetadataUpdatedEvent`   |

All events inherit the shared-kernel `DomainEvent` marker and are
immutable records.

## Invariants

Enforced inside the aggregate by `EnsureInvariants()` on every
`RaiseDomainEvent` pass:

1. **Title present** — a registered asset must have a non-empty title.
2. **Digest present** — a registered asset must have a non-empty
   SHA-256 content digest.
3. **Storage present** — a registered asset must declare an absolute
   storage URI and a positive byte size.
4. **Publishable only after availability** — an asset may only reach
   `Published` after having been in `Available` at least once.
5. **Tag uniqueness** — no duplicate tags on a single asset.
6. **Transition legality** — no illegal status transitions (enforced
   via `MediaAssetTransitionSpecification`).
7. **Archived is terminal** — no further mutations accepted when the
   asset is `Archived`.

Structural validation (title/description/digest/storage/tag format) is
enforced by the relevant value objects at construction time.

## Files

```
content-system/media/asset/
├── aggregate/
│   └── MediaAssetAggregate.cs
├── error/
│   └── MediaAssetErrors.cs
├── event/
│   ├── MediaAssetArchivedEvent.cs
│   ├── MediaAssetAvailableEvent.cs
│   ├── MediaAssetMetadataUpdatedEvent.cs
│   ├── MediaAssetProcessingStartedEvent.cs
│   ├── MediaAssetPublishedEvent.cs
│   ├── MediaAssetRegisteredEvent.cs
│   └── MediaAssetUnpublishedEvent.cs
├── specification/
│   ├── MediaAssetRegistrationSpecification.cs
│   └── MediaAssetTransitionSpecification.cs
├── value-object/
│   ├── ContentDigest.cs
│   ├── MediaAssetId.cs
│   ├── MediaAssetStatus.cs
│   ├── MediaDescription.cs
│   ├── MediaTag.cs
│   ├── MediaTitle.cs
│   ├── MediaType.cs
│   └── StorageLocation.cs
└── README.md
```

## Purity & Determinism

- Zero infrastructure, persistence, framework, or external service
  dependencies. References `Whycespace.Domain.SharedKernel.*` only.
- No `Guid.NewGuid()`, no `DateTime.UtcNow`, no `Random`. All
  identifiers and timestamps are supplied by callers (engine/runtime
  layer, where `IIdGenerator`/`IDeterministicIdEngine`/`IClock` live).
- Events are immutable records with `get; init;`-equivalent record
  semantics.
- Aggregate is sealed, single-rooted, and exposes state only via
  `{ get; private set; }` properties.
