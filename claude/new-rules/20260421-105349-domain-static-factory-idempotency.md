---
CLASSIFICATION: domain
SOURCE: E1→EX template alignment pass against content-system/document (CLAUDE.md $1 execution, 2026-04-21)
SEVERITY: S2
---

# Static-factory lifecycle-init is structurally idempotent — clarify guard + template

## DESCRIPTION

`claude/guards/domain.guard.md` rule `DOM-LIFECYCLE-INIT-IDEMPOTENT-01` and `claude/templates/delivery-pattern/01-domain-skeleton.md` §Aggregate-pattern item 3 both prescribe:

```csharp
public void OpenOrCreate(...)
{
    if (Version >= 0) throw <Aggregate>Errors.AlreadyInitialized();
    RaiseDomainEvent(...);
}
```

as the canonical lifecycle-init idempotency guard. The rule assumes an **instance-method** init pattern (economic exemplar: `CapitalAccountAggregate.Open(...)`, which is called on an instance that may or may not have had history rehydrated into it).

Template 01 §Aggregate-pattern item 2 simultaneously permits a **static-factory** init pattern (`Constructor private; static factory Create(...) or Open(...) returns the aggregate.`). Content-system/document adopts this pattern uniformly across all 10 populated BCs (e.g. `DocumentAggregate.Create(...)`, `DocumentFileAggregate.Register(...)`, `DocumentUploadAggregate.Request(...)`, `RetentionAggregate.Apply(...)`).

For the static-factory pattern, the `Version >= 0` check is structurally **dead code**: the factory always returns a freshly-constructed instance via the private parameterless constructor, so `Version` is always `-1` at the moment of the check. No second-initialisation path exists because static methods cannot be invoked on a rehydrated instance.

The two patterns therefore satisfy `DOM-LIFECYCLE-INIT-IDEMPOTENT-01` via different mechanisms:

- **Instance-method init** (economic exemplar) — requires runtime `Version >= 0` guard because the same instance may be called post-rehydration.
- **Static-factory init** (content-system/document) — satisfied by construction; a new instance is returned every call.

The current guard text only spells out the instance-method form, and the template 01 anti-drift checklist item "Aggregate has `Version >= 0` lifecycle-init guard" reads as universally mandatory — leaving the static-factory pattern in an ambiguous-conformance state.

## PROPOSED_RULE

### Option A — Update guard (preferred)

Amend `DOM-LIFECYCLE-INIT-IDEMPOTENT-01` in `domain.guard.md` to explicitly enumerate the two conformant patterns:

1. **Instance-method pattern:** `Open*/Create*/Initialize*` on an instance MUST check `Version >= 0` (or an equivalent `Already*Specification`) before raising the first event.
2. **Static-factory pattern:** `public static {Aggregate} Create*/Open*/Register*/Request*/Apply*(...)` MAY rely on structural idempotency — the factory returns a freshly-constructed instance and no explicit `Version >= 0` check is required. The BC README MUST document the static-factory choice so audits can distinguish "deliberate pattern" from "missing guard".

Update `01-domain-skeleton.md` §Aggregate-pattern item 3 to reference the two patterns consistently with the amended guard.

Update `05-quality-gates.md` Gate 1.10 to pass either form.

### Option B — Retrofit (not preferred)

Require all static factories to add a dead-code `if (aggregate.Version >= 0) throw ...AlreadyInitialized();` after `new X()`, purely for mechanical uniformity. Increases ceremony without reducing risk.

## EVIDENCE

- Economic exemplar: instance-method pattern on `CapitalAccountAggregate.Open(...)`; only 2 of ~40 economic aggregates actually carry the `Version >= 0` check in code — suggesting even the exemplar is inconsistent with its own documented rule, and Option A would bring all verticals under a single clear rule.
- Content-system alignment pass (2026-04-21) covered **all three populated contexts** and found **every** populated aggregate uses the static-factory pattern. **26 aggregates across 26 BCs, zero `Version >= 0` hits, zero instance-method init paths:**
  - `document` (10 BCs): `DocumentAggregate` (Create), `DocumentBundleAggregate` (Create), `DocumentFileAggregate` (Register), `DocumentRecordAggregate` (Create), `DocumentTemplateAggregate` (Create), `DocumentMetadataAggregate` (Create), `RetentionAggregate` (Apply), `DocumentUploadAggregate` (Request), `DocumentProcessingAggregate` (Request), `DocumentVersionAggregate` (Create).
  - `media` (7 BCs): `AssetAggregate` (Create), `SubtitleAggregate` (Create), `TranscriptAggregate` (Create), `MediaMetadataAggregate` (Create), `MediaIngestAggregate` (Request), `MediaVersionAggregate` (Create), `MediaProcessingAggregate` (Request).
  - `streaming` (9 BCs): `StreamAccessAggregate` (Grant), `ObservabilityAggregate` (Capture), `ArchiveAggregate` (Start), `BroadcastAggregate` (Create), `SessionAggregate` (Open), `PlaybackAggregate` (Create), `ChannelAggregate` (Create), `ManifestAggregate` (Create), `StreamAggregate` (Create).
- Remediation applied: each of the 26 BC READMEs now carries a "Template conformance (E1→EX `01-domain-skeleton`)" section explicitly declaring the static-factory choice + factory verb + specification inventory + rationale for the missing `Version >= 0` guard. Consistent WHEN-NEEDED folder disposition across all BCs: `entity/` omitted (`.gitkeep`), `service/` omitted (`.gitkeep`), `specification/` populated — i.e. the same pattern-signature appears in every populated BC, strengthening the case that static-factory is the de-facto content-system convention, not an accident.
- Related S1 remediated in the same pass: `INV-REPLAY-LOSSLESS-VALUEOBJECT-01` — **136 events** under `src/domain/content-system/` (53 document + 34 media + 49 streaming) were missing `[property: JsonPropertyName("AggregateId")]` on the aggregate-id VO first-parameter plus the `System.Text.Json.Serialization` import. All 136 fixed; `dotnet build src/domain/Whycespace.Domain.csproj` succeeds with 0 warnings / 0 errors post-fix.
- Post-fix mechanical audit sweep over content-system (all 3 populated contexts): zero hits on `Guid.NewGuid`, `DateTime.Now/UtcNow`, `new Random`, `Microsoft.Extensions.DependencyInjection`, `throw new (ArgumentException|ArgumentNullException|InvalidOperationException|NotImplementedException)`, `_uncommittedEvents`, `// TODO|FIXME|HACK`, `public Guid \w+Id|string \w+Id[,)]`. All 4 Gate 1 MUST folders present for every BC; namespace conformance `Whycespace.Domain.ContentSystem.*` is 100%.
