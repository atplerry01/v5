# Phase 2.6 — Content-System Canonical Realignment — CLOSED

Closed: 2026-04-20 09:45:58 UTC
Controlling baseline artifacts (authoritative):
- `phase-2.6-baseline.md`          (CS.0)
- `phase-2.6-final-audit.md`       (CS.12 — all structural gates green)
- `phase-2.6-cs-13A-report.md`     (CS.13A identifier renames + AssetKind)
- `phase-2.6-cs-13B-plan.md`       (CS.13B planned follow-up gate)

Status: **COMPLETE through CS.13A.** No further structural or identifier work
in this phase. Do not reopen unless a new audit finds a concrete defect.

---

## What is now canonically complete

**Topology.** 4 contexts, 18 groups, 55 leaves, 385 artifact subfolders.
Every leaf carries the 7 mandatory artifact folders (aggregate, entity, error,
event, service, specification, value-object). Context roster is final:

- `document/` — 7 groups, 20 leaves.
- `media/` — 8 groups, 18 leaves.
- `streaming/` — 4 groups, 14 leaves.
- `shared/` — 3 groups, 3 leaves.

**Namespaces.** All C# namespaces under `Whycespace.Domain.ContentSystem.*`
reflect the canonical post-realignment layout. Zero stale legacy-group
namespace fragments (`ContentArtifact`, `CompanionArtifact`,
`DeliveryArtifact`, `Control`, `PersistenceAndObservability`, `Lifecycle` as
parent segments) remain.

**Identifier vocabulary** (aligned in CS.13A):
- `LiveStreamAggregate` → `BroadcastAggregate` (+ all LiveStream* events/VOs → Broadcast*)
- `StreamSessionAggregate` → `SessionAggregate`
- `RecordingAggregate` → `ArchiveAggregate` (+ cross-leaf `RecordingRef` → `ArchiveRef`)
- `MetricsAggregate` → `ObservabilityAggregate`
- `MediaUpload*` → `MediaIngest*`

**AssetKind alignment** (CS.13A Wave 6, conservative+additive):
- `AssetKind` VO (Other/Audio/Video/Image)
- `AssetKindAssignedEvent`
- `AssetAggregate.Kind` property and `AssignKind` command
- `AssetCreatedEvent` shape unchanged — replay-decodable.

**Guard compliance** (verified at CS.12):
- DS-R3 (classification/context/domain-group/domain) ✅
- DS-R3a (per-context atomicity) ✅ 0 violations
- DS-R8 (DomainRoute 3-tuple stability) ✅ 0 live callers
- GUARD-LAYER-MODEL-01 (exactly 4 guards, no subdirs) ✅

**Deferred-item ledger (closed this phase):**
- §DF-01 (playback fate) — CLOSED: Playback = availability descriptor →
  `stream-core/availability/` (MOVE only; naming intentionally retained).
- §DF-02 (metrics home) — CLOSED: → `delivery-governance/observability/`.
- §DF-03 (media VO split) — CLOSED: §CD-03 refinement applied
  (intrinsic→asset; evaluative→quality).
- §DF-04 (retention scope) — CLOSED: per-context only, no shared promotion.
- §DF-06 (recording partition) — CLOSED: all events broadcast-side → archive;
  replay = scaffold.

---

## What remains deferred

**CS.13B — Band-F semantic README completion.** Planned as a separate gate.
Scope-locked to documentation only (no code). See
`phase-2.6-cs-13B-plan.md`.

**§DF-05 — DocumentFile integrity boundary.** Non-blocking. Current posture:
`DocumentFile.IntegrityStatus` VO retained as cached view state; verification
lifecycle becomes canonical on the (scaffolded) `integrity/` domain when that
domain is implemented. Resolve during the integrity-feature phase.

**§DF-07 — DocumentUpload → DocumentProcessing handoff event.** Non-blocking.
`DocumentUploadProcessingStartedEvent` remains on the upload aggregate. Clean
up when the upload/processing boundary is formalised by a feature phase.

**§DF-08 — subtitle/transcript shared behaviour.** Informational — no
doctrinal action required.

**Mirror-layer scaffolding (DS-R4).** Pre-existing drift captured at CS.0.
Content-system has no mirror layers under `src/engines/T2E/content/`,
`src/projections/content/`, `infrastructure/policy/domain/content/`, or
`infrastructure/event-fabric/kafka/topics/content/`. A follow-up phase
(P2.7+) must scaffold mirror layers **against the post-realignment tree**,
not the legacy layout.

---

## What is intentionally preserved

**`PlaybackAggregate` naming.** The aggregate lives at the canonical folder
`streaming/stream-core/availability/` and the namespace
`ContentSystem.Streaming.StreamCore.Availability`, but the C# identifiers
(`PlaybackAggregate`, `Playback*Event`, `PlaybackId`, `PlaybackMode`,
`PlaybackSourceRef/Kind`, `PlaybackStatus`, `PlaybackWindow`,
`CanEnablePlaybackSpecification`) remain unchanged.

Rationale: §DF-01 verdict flagged the identifier rename as
semantically fragile; user direction at CS.13A reinforced "do not rename
for symmetry." The rename will be reconsidered only by a future dedicated
inspection gate producing a stronger canonical reason — not by routine
cleanup. Until then this naming asymmetry is **accepted intentional
state**, not drift.

**`MetricsSnapshot` value objects inside the observability leaf.** The
aggregate/events/status/window were renamed Metrics→Observability, but
sub-VOs — `BitrateMeasurement`, `DropCount`, `ErrorCount`,
`LatencyMeasurement`, `PlaybackCount`, `ViewerCount`, and the container
`ObservabilitySnapshot` — describe data shape, not aggregate identity.
These names are stable and meaningful.

**`StreamSessionRef` inside `archive/value-object/`.** Although the session
aggregate was renamed Session, the cross-leaf reference VO type name
`StreamSessionRef` is a ref-type name (not an aggregate/event/VO name from
the session leaf itself). It remains distinctive and is preserved.

**Kind-qualified intrinsic VOs not materialised on `AssetAggregate` state.**
`AudioDuration`, `ChannelCount`, `AudioFormat`, `VideoDuration`, `FrameRate`,
`VideoDimensions`, `ImageDimensions`, `ImageOrientation` live as VO files
under `asset/value-object/` but are NOT stored on `AssetAggregate`. The
aggregate holds `AssetKind` only. Callers honour the §CD-03 split rule at
application-level construction. Future evolution may absorb VOs onto state
with kind-consistency invariants — out of scope for Phase 2.6.

---

## What future audits should verify

1. **Topology stability.** 55 leaves, 7-folder integrity per leaf, 4-context
   atomicity (DS-R3a). Any regression is a P2.6 revert and must reopen the
   phase.

2. **Namespace hygiene.** No reappearance of the 6 retired group-namespace
   fragments. Script:
   `grep -nE 'ContentSystem\.(Document|Media|Streaming)\.(ContentArtifact|CompanionArtifact|DeliveryArtifact|Control|PersistenceAndObservability|Lifecycle)\.' src/**/*.cs`

3. **Identifier hygiene.** No reappearance of CS.13A retired identifiers:
   `LiveStream(Aggregate|Id|Status|Errors|\w*Event)`,
   `StreamSession(Aggregate|Id|Errors|\w*Event)`,
   `Recording(Aggregate|Id|Errors|OutputRef|FailureReason|Status|Ref|\w*Event)`,
   `Metrics(Aggregate|Id|Errors|Snapshot|Status|Window|\w*Event)`,
   `MediaUpload[A-Z]`,
   `CanStartLiveStream|CanFinalizeRecording|CanUpdateMetrics|CanCompleteMediaUpload|LiveBroadcastWindow`.

4. **DomainRoute 3-tuple stability.** `DomainRoute("content", ...)` source-
   code call count must remain 0 until the mirror-layer phase introduces
   live callers. Any appearance before that signals premature wiring.

5. **§CD-16 disambiguation discipline.** `DocumentAggregate.Classification`
   VO must never be mutated by local command — only by events from
   `document/governance/classification/`. Audit for any direct mutation
   command pattern.

6. **§CD-03 split discipline.** Any new media VO is classified at review
   time as intrinsic (→ asset) or evaluative (→ quality). Audit for VOs
   added to the wrong leaf.

7. **AssetCreatedEvent shape invariance.** The event record must NOT gain
   an `AssetKind` field — replay decodability depends on the current shape.
   Kind-on-create is set via a separate `AssignKind` invocation after
   creation.

8. **`PlaybackAggregate` non-rename preservation.** Until a dedicated
   inspection gate issues a reversal, do not rename PlaybackAggregate or
   its events/VOs. Audit for accidental "for symmetry" renames.

9. **Shared-context gating.** Additions to `shared/` must pass the three
   tests stated in `shared/README.md`. Audit for half-justified additions.

10. **CS.13B documentation scope.** When CS.13B executes, the audit must
    verify it touched ONLY `*.md` files (no `*.cs`) and introduced no
    structural or identifier changes.

---

## Closure statement

Phase 2.6 content-system canonical realignment is **CLOSED**. The domain-
layer tree of `content-system/` now matches the target canonical model
exactly, with identifier vocabulary aligned to the target. `PlaybackAggregate`
is the one intentional deviation — accepted, documented, and protected from
routine cleanup. Semantic README completion is planned as CS.13B and does
not block this closure.
