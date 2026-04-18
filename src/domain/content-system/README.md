# content-system

Bounded-context map for the content-system classification. This domain
subtree owns content authoring, delivery, interaction, organization,
engagement, lifecycle, access, discovery, monetization, and moderation —
the complete domain-model surface that powers WhatsApp-style messaging,
TikTok-style engagement and discovery, Netflix-style streaming and access,
and Udemy-style organization and lifecycle scenarios.

This README documents the target shape after the DDD refactor captured
in `claude/project-prompts/20260418-003131-content-content-system-ddd-refactor.md`
and executed through `pipeline/execution_context.md` against
`pipeline/generic-refactor-prompt.md`.

---

## Bounded contexts

```
content-system/
├── content/              Owns content TRUTH
│   ├── document/         Text / structured-document aggregates
│   └── media/            Video / image / audio / manifest aggregates
│
├── delivery/             Owns streaming / session transport only
│   └── stream/           Playback session aggregate — carries ContentRef +
│                         ManifestLocation only; NO media metadata
│
├── interaction/          Owns real-time participant-to-participant exchange
│   └── conversation/     Conversations, messages, participants
│                         (parked siblings: presence/, call/ — untouched
│                         by this refactor, kept for future phases)
│
├── organization/         Owns user-authored groupings of content
│   └── collection/       Ordered lists of ContentRef — playlists, courses,
│                         albums, feeds-by-author
│
├── engagement/           Owns actor-to-content reactions + activity
│   ├── reaction/         Like / heart / bookmark / switchable reaction
│   ├── comment/          Threaded comments on any ContentRef
│   └── view/             View records (engagement telemetry, thin
│                         aggregate — heavy aggregation is projection-side)
│                         (parked sibling: community/ — untouched)
│
├── lifecycle/            Owns publication state machine
│   └── publication/      Draft → Review → Scheduled → Published →
│                         Archived → Deleted. SOLE emitter of
│                         ContentPublishedEvent
│
├── access/               Owns who can access what
│   ├── rights/           License / territory / exclusivity on ContentRef
│   └── entitlement/      Principal × ContentRef grants
│                         (migrated from monetization/entitlement)
│
├── discovery/            Owns feed + search DOMAIN surface only
│   ├── feed/             FeedDefinition aggregate (ranking policy) —
│   │                     ranked items live in projections
│   └── search/           SavedQuery aggregate — index lives in projections
│                         (both migrated from engagement/{feed,search})
│
├── monetization/         Owns billing + payout
│   ├── subscription/     Principal × Plan, billing cycle, status machine.
│   │                     Does NOT construct entitlements — a saga at the
│   │                     engine layer bridges SubscriptionActivatedEvent
│   │                     → EntitlementGrantedEvent
│   └── payout/           Creator payouts (non-overlapping periods per
│                         recipient, executed / reversed)
│                         (parked sibling: pricing/ — untouched)
│
├── moderation/           Owns content safety enforcement
│   ├── report/           Report intake + triage
│   └── action/           Enforcement action execution
│                         (split from the former governance/moderation/)
│                         (parked siblings under governance/: compliance/,
│                         content-policy/ — untouched)
│
└── shared/               Cross-domain kernel
    └── content-ref/      ContentRef + ContentType + ContentId +
                          ContentVersion — the ONLY import permitted
                          across domain boundaries inside content-system
```

---

## Cross-domain rules

### 1. ContentRef is the only cross-domain reference

```csharp
using Whycespace.Domain.ContentSystem.Shared.ContentRef;

public sealed class ReactionAggregate : AggregateRoot
{
    public ContentRef Target { get; private set; } // ← just the ref
    // ...
}
```

Domains may NOT import each other's internal types. An engagement
aggregate may not import a content/media type; a delivery aggregate may
not import a content/media type; a monetization aggregate may not import
an access type. Enforced via post-refactor grep sweeps in Stage G of the
refactor pipeline and by the domain guard thereafter.

### 2. Events are the only cross-domain runtime channel

State changes emit events (`{Domain}{Action}Event`). Other domains react
via sagas / handlers / projections in the engine and projection layers —
never via direct aggregate-to-aggregate calls. The domain layer itself
does not dispatch or subscribe; those wirings live in `engines/`,
`runtime/`, and `projections/` (out of scope for this refactor).

### 3. Lifecycle emitter exclusivity

`ContentPublishedEvent` is raised ONLY by `PublicationAggregate`.
`content/document` and `content/media` never handle publish logic — they
emit readiness events (`DocumentRevisedEvent`, `MediaFinalizedEvent`) and
publication is the separate concern of `lifecycle/publication`.

### 4. Streaming isolation

`delivery/stream` stores no media metadata — only `ContentRef`, a
`ManifestLocation` snapshot recorded at session start, and the snapshot
of the authorizing `EntitlementId`. MediaType, Duration, Codec,
Resolution, and any content-owned VO are forbidden inside
`delivery/stream/**`.

### 5. Access decoupling

`monetization/subscription` never constructs or calls
`access/entitlement`. A saga in the engine layer (out of this refactor's
scope) reacts to `SubscriptionActivatedEvent` and calls
`EntitlementAggregate.Grant`.

---

## Internal domain shape

Every target domain follows the canonical seven-folder structure:

```
{domain}/
├── aggregate/        aggregate roots (one or more per domain)
├── entity/           sub-entities of aggregate roots
├── error/            domain-specific errors (DomainException-derived)
├── event/            domain events ({Domain}{Action}Event records)
├── service/          domain services (pure, no infrastructure)
├── specification/    reusable business-rule checks
├── value-object/     immutable VOs (record structs)
└── README.md         per-domain purpose + use-case documentation
```

Folders remain present even when currently empty, tracked via
`.gitkeep`, so the canonical shape is discoverable without reading code.

---

## Product scenarios powered

| Scenario                       | Primary domains                                                  |
|--------------------------------|------------------------------------------------------------------|
| WhatsApp-style messaging       | interaction/conversation, content/media, access/entitlement, delivery/stream |
| TikTok-style short-form        | content/media, lifecycle/publication, discovery/feed, engagement/{reaction,comment,view}, delivery/stream |
| Netflix-style streaming        | content/media, access/{rights,entitlement}, monetization/subscription, delivery/stream |
| Udemy-style courses            | content/{document,media}, organization/collection, lifecycle/publication, access/entitlement, monetization/subscription, engagement/view |

---

## Parked domains (byte-identical across this refactor)

These exist in the tree today and remain untouched by this refactor. They
are legitimate domains outside the target shape's concerns; future
refactor phases may absorb or retire them separately.

- `interaction/presence/`
- `interaction/call/`
- `engagement/community/`
- `governance/compliance/`
- `governance/content-policy/`
- `monetization/pricing/`

A diff of any parked path before and after this refactor MUST be empty.

---

## Canonical conventions (mirror repo-wide)

- Aggregate base: `Whycespace.Domain.SharedKernel.Primitives.Kernel.AggregateRoot`
- Guard helper: `Whycespace.Domain.SharedKernel.Primitives.Kernel.Guard.Against(bool, string)`
- Timestamp VO: `Whycespace.Domain.SharedKernel.Primitives.Kernel.Timestamp`
- Aggregate style: private parameterless ctor, static `Create(...)` factories,
  command methods raise events via `RaiseDomainEvent` (which runs
  `Apply` → `EnsureInvariants` → appends to event list)
- Namespaces: `Whycespace.Domain.ContentSystem.{Context}.{Domain}` — the
  `aggregate/`, `value-object/`, etc. folder does not appear in the
  namespace

Reference aggregate to mirror:
`src/domain/economic-system/ledger/journal/aggregate/JournalAggregate.cs`
