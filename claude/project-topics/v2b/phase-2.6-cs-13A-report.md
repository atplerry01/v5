# Phase 2.6 — CS.13A Change Report (identifier rename pass)

Executed: 2026-04-20 09:42:09 UTC
Scope: rename-only — no structural tree changes, no Band-F README sweep.
Controlling baseline: user's Option B narrowed scoping (turn 5).

---

## 1. Rename map

| Wave | From                              | To                                    | Files renamed | Identifiers rewrote | Verdict |
|------|-----------------------------------|---------------------------------------|---------------|---------------------|---------|
| 1    | LiveStream*                       | Broadcast*                            | 13            | 13                  | ✅ clean |
| 2    | StreamSession*                    | Session*                              | 10            | 11                  | ✅ clean |
| 3    | Recording* (+ RecordingRef)       | Archive* (+ ArchiveRef)               | 13            | 15                  | ✅ clean |
| 4    | Metrics* (aggregate/events/VOs)   | Observability*                        | 11            | 11                  | ✅ clean |
| 5    | MediaUpload*                      | MediaIngest*                          | 15            | 15                  | ✅ clean |
| 6    | AssetKind VO + alignment wiring   | (new VO, new event, aggregate extended) | 2 new + 2 edited | n/a                | ✅ clean |
| **Totals** |                             |                                       | **64 renamed + 2 new + 2 edited** | **65**          |         |

## 2. Blocked items (intentional)

### PlaybackAggregate → AvailabilityAggregate — BLOCKED
- Flagged in §DF-01 verdict (§claude/new-rules/20260420-091320-audits.md):
  "RECOMMEND Playback prefix be RETAINED during the move — rename the
  PHYSICAL location (playback → availability) but keep aggregate + event
  class names prefixed Playback* for the first pass. Reason: minimises
  event-stream disruption during realignment; identifier rename is a
  semantic change best made as a deliberate, isolated decision."
- User reinforcement (turn 5): "only rename `PlaybackAggregate ->
  AvailabilityAggregate` if that was definitively proven earlier. That one
  is the most semantically fragile of the set, so it should not be renamed
  just for symmetry."
- Verdict applied: **NO rename executed this gate.** Folder + namespace
  (`stream-core/availability/`, `...StreamCore.Availability`) remain as
  moved in CS.5; class identifiers remain `PlaybackAggregate`, `Playback*Event`,
  `PlaybackId`, `PlaybackMode`, `PlaybackSourceRef/Kind`, `PlaybackStatus`,
  `PlaybackWindow`. `CanEnablePlaybackSpecification` unchanged.

## 3. Wave 6 — AssetKind alignment details

Files added:
- `src/domain/content-system/media/core-object/asset/value-object/AssetKind.cs`
  (enum: Other / Audio / Video / Image)
- `src/domain/content-system/media/core-object/asset/event/AssetKindAssignedEvent.cs`

Files edited:
- `src/domain/content-system/media/core-object/asset/aggregate/AssetAggregate.cs`
  - Added `public AssetKind Kind { get; private set; }`
  - Added `public void AssignKind(AssetKind newKind, Timestamp assignedAt)`
  - Added Apply case for `AssetKindAssignedEvent`
  - Added `Kind = AssetKind.Other` initialization on `AssetCreatedEvent` Apply
- `src/domain/content-system/media/core-object/asset/error/AssetErrors.cs`
  - Added `CannotAssignKindToRetiredAsset()` and `AssetKindUnchanged()` factories
- `src/domain/content-system/media/core-object/asset/README.md`
  - Rewrote Aggregate root / VO list / Events / Owns sections to reflect Kind

**Design discipline preserved:**
- `AssetCreatedEvent` shape UNCHANGED — Kind is assigned via a separate
  command/event pair. Historical `AssetCreatedEvent` streams remain
  decodable.
- Kind-qualified intrinsic VOs (AudioDuration, ChannelCount, FrameRate,
  ImageDimensions, etc.) remain available on `asset/value-object/` as
  types; they are NOT materialised on AssetAggregate's state in this gate.
  The aggregate knows its Kind but does not hold the per-kind descriptors.
  Future work may absorb them with kind-consistency invariants.

## 4. Verification

| Check                                                     | Result |
|-----------------------------------------------------------|--------|
| 0 stale legacy identifiers (LiveStream/StreamSession/Recording/Metrics/MediaUpload) across all .cs | ✅ |
| 55 leaves intact; 7 artifact folders each                 | ✅ |
| 0 live `DomainRoute("content", ...)` callers in source    | ✅ |
| AssetKind VO + event + aggregate wiring present           | ✅ |
| Band-S README patches: asset updated                      | ✅ |
| Structural tree unchanged (no moves, no new groups)       | ✅ |

## 5. Files touched

Working-tree overall change count: ~542 entries (includes all prior gates).
Rename-specific lines this gate: 64 rename ops + 2 adds + 2 edits (+ README patches).

## 6. Follow-up — CS.13B (separate planned gate)

Scope deferred to its own dedicated gate:

- Band-F full-semantic READMEs across all 55 leaves
  - Lifecycle/state diagram per leaf
  - Event example list
  - Upstream / downstream reference map
- Group-level invariant and cohesion rationale
- Context-level non-inclusion notes
- Potential follow-on for the BLOCKED Playback→Availability rename —
  requires a separate gate with a second inspection + explicit deliberation,
  not an automatic-symmetry rename.

## 7. Verdict

**CS.13A: PASS.** Structural tree green, identifier vocabulary aligned to the
canonical target for all 5 approved rename waves + AssetKind alignment.
PlaybackAggregate rename intentionally held back per §DF-01 verdict.
