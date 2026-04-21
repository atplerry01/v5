---
type: audit-sweep-output
audit: claude/audits/e1-ex-domain.audit.md
template: claude/templates/delivery-pattern/ (v1.0 locked 2026-04-19)
scope: src/domain/content-system/** (document + media + streaming populated contexts; shared + invariant D0 out-of-scope)
run: CLAUDE.md §1 execution of claude/templates/delivery-pattern/_enry_prompt_v2.md
status: PASS
---

# E1→EX Domain Conformance — content-system alignment sweep

```
E1XD-AUDIT-RUN:
  timestamp: 2026-04-21T11:20:26Z
  classification: content-system
  contexts_scanned: [document, media, streaming, shared, invariant]
  populated_contexts: [document, media, streaming]
  bcs_scanned: 26
  bcs_promoted_ready_for_D1: 26

  findings_pre_remediation:
    - rule: E1XD-EVENT-JPN-01 (INV-REPLAY-LOSSLESS-VALUEOBJECT-01)
      severity: S1
      count: 136
      scope: every domain event record under content-system/{document,media,streaming}/**/event/*.cs
      evidence: 0 files had [property: JsonPropertyName("AggregateId")] on the aggregate-id VO first parameter; no file imported System.Text.Json.Serialization
      remediation: added [property: JsonPropertyName("AggregateId")] + using System.Text.Json.Serialization; to all 136 events via two-edit mechanical pass

    - rule: E1XD-FOLDER-OPT-01 (WHEN-NEEDED README justification)
      severity: S2
      count: 26
      scope: populated BCs under content-system — WHEN-NEEDED folders entity/ and service/ were physically present with .gitkeep but not justified-as-omitted in the BC README (template 01 says .gitkeep is acceptable for D0; D1 requires either content or README justification)
      remediation: appended "Template conformance (E1→EX 01-domain-skeleton)" footer to every populated BC README declaring MUST/WHEN-NEEDED folder disposition, specification inventory, and static-factory lifecycle-init rationale

    - rule: drift discovered — pattern-vs-template mismatch
      severity: S2
      count: 26 (all populated BCs)
      scope: content-system uses static-factory init pattern (public static {Aggregate} {Verb}(…)), not the instance-method Open(this…) pattern that DOM-LIFECYCLE-INIT-IDEMPOTENT-01 and template 01 item 3 describe as canonical
      remediation: NONE (code — static-factory is structurally idempotent by construction and template 01 item 2 explicitly permits it); captured as new-rule proposal to amend the guard + template to enumerate both patterns as conformant

  findings_post_remediation:
    purity_checks:
      guid_newguid: 0
      datetime_now_utcnow: 0
      new_random: 0
      microsoft_extensions_di: 0
      bcl_exception_throws: 0
      uncommitted_events_manual: 0
      todo_fixme_hack: 0
      raw_guid_string_id_primitives: 0
    folder_checks:
      must_folders_present_per_bc: 26/26
      when_needed_folders_justified_or_present: 26/26
    naming_checks:
      aggregate_name_conformance: 26/26
      event_name_conformance: 136/136
      namespace_conformance: 100%
    event_checks:
      json_propertyname_aggregateid_present: 136/136
      past_tense_verb: 136/136
      domain_event_base: 136/136
    aggregate_checks:
      aggregateroot_inheritance: 26/26
      raisedomainEvent_usage: 26/26
      lifecycle_init_idempotency: 26/26 (satisfied by static-factory construction; see new-rule capture)

  build_verification:
    project: src/domain/Whycespace.Domain.csproj
    result: success
    warnings: 0
    errors: 0
    timestamp: 2026-04-21T11:12:00Z

  s0_count: 0
  s1_count: 0   # (136 pre-remediation, 0 post-remediation)
  s2_count: 0   # (all S2 documented or captured as new-rule)
  verdict: PASS

  new_rules_captured:
    - claude/new-rules/20260421-105349-domain-static-factory-idempotency.md

  files_modified:
    event_records: 136  # 53 document + 34 media + 49 streaming
    bc_readmes: 26      # 10 document + 7 media + 9 streaming
    total: 162

  out_of_scope_deferred:
    - content-system/shared/** (0 .cs files — all D0 scaffold)
    - content-system/invariant/ownership/policy/** (2 .cs files, policy-only; non-standard BC topology)
    - D0-scaffold BCs within populated contexts (document: attachment, classification, moderation, import, integrity, provenance, publication, review, export, preview; plus media + streaming scaffolds)
    - Phases 7–18 (engine / runtime / policy / projections / API / tests / docs) — session scoped to domain-model only
```

## Per-BC inventory (26 populated)

### document (10 BCs)

| BC | Aggregate | Factory verb | Specs |
|---|---|---|---|
| core-object/bundle | `DocumentBundleAggregate` | `Create` | `CanModifyBundleSpecification` |
| core-object/document | `DocumentAggregate` | `Create` | `CanArchiveSpecification` |
| core-object/file | `DocumentFileAggregate` | `Register` | `CanVerifyDocumentFileIntegritySpecification` |
| core-object/record | `DocumentRecordAggregate` | `Create` | `CanCloseRecordSpecification` |
| core-object/template | `DocumentTemplateAggregate` | `Create` | `CanActivateTemplateSpecification` |
| descriptor/metadata | `DocumentMetadataAggregate` | `Create` | `CanModifyMetadataSpecification` |
| governance/retention | `RetentionAggregate` | `Apply` | `CanReleaseRetentionSpecification` |
| intake/upload | `DocumentUploadAggregate` | `Request` | `CanCompleteDocumentUploadSpecification` |
| lifecycle-change/processing | `DocumentProcessingAggregate` | `Request` | `CanStartProcessingSpecification`, `CanCompleteProcessingSpecification` |
| lifecycle-change/version | `DocumentVersionAggregate` | `Create` | `CanActivateSpecification`, `CanSupersedeSpecification` |

### media (7 BCs)

| BC | Aggregate | Factory verb | Specs |
|---|---|---|---|
| core-object/asset | `AssetAggregate` | `Create` | `CanRetireSpecification` |
| core-object/subtitle | `SubtitleAggregate` | `Create` | `CanModifySubtitleSpecification` |
| core-object/transcript | `TranscriptAggregate` | `Create` | `CanModifyTranscriptSpecification` |
| descriptor/metadata | `MediaMetadataAggregate` | `Create` | `CanModifyMediaMetadataSpecification` |
| intake/ingest | `MediaIngestAggregate` | `Request` | `CanCompleteMediaIngestSpecification` |
| lifecycle-change/version | `MediaVersionAggregate` | `Create` | `CanActivateMediaVersionSpecification`, `CanSupersedeMediaVersionSpecification` |
| technical-processing/processing | `MediaProcessingAggregate` | `Request` | `CanStartMediaProcessingSpecification`, `CanCompleteMediaProcessingSpecification` |

### streaming (9 BCs)

| BC | Aggregate | Factory verb | Specs |
|---|---|---|---|
| delivery-governance/access | `StreamAccessAggregate` | `Grant` | `CanRevokeAccessSpecification` |
| delivery-governance/observability | `ObservabilityAggregate` | `Capture` | `CanUpdateObservabilitySpecification` |
| live-streaming/archive | `ArchiveAggregate` | `Start` | `CanFinalizeArchiveSpecification` |
| live-streaming/broadcast | `BroadcastAggregate` | `Create` | `CanStartBroadcastSpecification` |
| playback-consumption/session | `SessionAggregate` | `Open` | `CanActivateSessionSpecification` |
| stream-core/availability | `PlaybackAggregate` | `Create` | `CanEnablePlaybackSpecification` |
| stream-core/channel | `ChannelAggregate` | `Create` | `CanEnableChannelSpecification` |
| stream-core/manifest | `ManifestAggregate` | `Create` | `CanPublishManifestSpecification` |
| stream-core/stream | `StreamAggregate` | `Create` | `CanActivateStreamSpecification`, `CanEndStreamSpecification` |

## Anchoring commands (re-verification)

```bash
# 1. JsonPropertyName coverage
grep -rc 'JsonPropertyName("AggregateId")' src/domain/content-system/
# expected: 136 across 136 files (53 document + 34 media + 49 streaming)

# 2. Purity sweep
grep -rEn 'Guid\.NewGuid|DateTime\.(Now|UtcNow)|new Random|Microsoft\.Extensions\.DependencyInjection|throw new (ArgumentException|ArgumentNullException|InvalidOperationException|NotImplementedException)|_uncommittedEvents|// (TODO|FIXME|HACK)' src/domain/content-system/
# expected: 0 matches

# 3. Raw-id typing
grep -rEn 'public Guid \w+Id|Guid \w+Id[,)]|string \w+Id[,)]' src/domain/content-system/
# expected: 0 matches

# 4. Template-conformance README footers
grep -rc '## Template conformance' src/domain/content-system/
# expected: 26 across 26 files

# 5. Build
dotnet build src/domain/Whycespace.Domain.csproj
# expected: 0 warnings / 0 errors
```

## Next steps (out of this session's scope)

1. **Promote D0 scaffolds** → author aggregates for the 10 document scaffold BCs (attachment, classification, moderation, import, integrity, provenance, publication, review, export, preview) plus remaining media/streaming scaffolds; each becomes a fresh Gate-1 pass.
2. **Phases 7–18** for populated BCs — engines, runtime wiring, policy, projections, API, three-tier tests, docs.
3. **Amend `domain.guard.md`** per new-rule `20260421-105349-domain-static-factory-idempotency.md` Option A — enumerate instance-method and static-factory patterns as independently conformant under `DOM-LIFECYCLE-INIT-IDEMPOTENT-01`. Update template 01 item 3 and Gate 1.10 to match.
