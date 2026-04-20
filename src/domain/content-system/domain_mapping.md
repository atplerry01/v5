# content-system — canonical domain map
Status: proposed canonical outline
Purpose: content-system is the authoritative source of truth for all content objects, their lifecycle, structure, delivery readiness, governance state, and evidence hooks. Other systems consume from it but do not own content truth.

---

## A. document context

### A1. intake domain group
#### 1. upload
Owns:
- upload request lifecycle
- acceptance / rejection
- intake session state
- upload channel/source registration
- initial file registration

Emits:
- DocumentUploadRequested
- DocumentUploadAccepted
- DocumentUploadRejected
- DocumentUploadStarted
- DocumentUploadCompleted
- DocumentUploadFailed
- DocumentUploadCancelled

Consumed by:
- media.processing
- document.integrity
- document.file
- document.governance
- operational intake workflows

#### 2. import
Owns:
- external source ingestion
- batch import jobs
- migration/import provenance
- source-system linkage

Emits:
- DocumentImportRequested
- DocumentImportStarted
- DocumentImported
- DocumentImportFailed
- DocumentImportCancelled

Consumed by:
- document.record
- document.provenance
- document.version
- operational migration flows

---

### A2. core-object domain group
#### 3. file
Owns:
- raw file object
- file metadata
- mime/content-type
- size
- storage pointer
- canonical file identity

Emits:
- DocumentFileRegistered
- DocumentFileMetadataUpdated
- DocumentFileStorageLinked
- DocumentFileReplaced
- DocumentFileRemoved

Consumed by:
- document.integrity
- document.version
- document.preview
- document.retention
- media.asset when promoted or linked

#### 4. document
Owns:
- canonical document aggregate
- document title/description
- document classification
- author/origin linkage
- language and metadata truth
- current state and active version

Emits:
- DocumentCreated
- DocumentMetadataUpdated
- DocumentReclassified
- DocumentLanguageTagged
- DocumentStateChanged
- DocumentArchived
- DocumentRestored

Consumed by:
- search/catalog systems
- governance/policy systems
- operational use-case systems
- external publication consumers

#### 5. attachment
Owns:
- attachment-specific content object
- attachment-to-parent linkage
- attachment visibility/state
- attachment packaging rules

Emits:
- AttachmentLinked
- AttachmentUnlinked
- AttachmentStateChanged
- AttachmentVisibilityChanged

Consumed by:
- messaging-like systems
- operational workflow systems
- records/case systems

#### 6. record
Owns:
- formal record state
- record designation
- record immutability or control posture
- record business classification

Emits:
- RecordDeclared
- RecordUpdated
- RecordLocked
- RecordReleased
- RecordArchived

Consumed by:
- governance
- retention
- legal/compliance systems
- audit/reporting systems

#### 7. bundle
Owns:
- multi-document packaging
- bundle membership
- bundle ordering
- exportable grouped content

Emits:
- BundleCreated
- BundleItemAdded
- BundleItemRemoved
- BundleReordered
- BundlePublished
- BundleArchived

Consumed by:
- learning/course consumers
- publication/catalog consumers
- export/distribution consumers

#### 8. template
Owns:
- reusable document template truth
- template versioning
- template structure
- template activation state

Emits:
- TemplateCreated
- TemplateUpdated
- TemplateVersionPublished
- TemplateDeprecated
- TemplateRetired

Consumed by:
- operational document generation
- drafting/proposal systems
- internal authoring workflows

---

### A3. integrity-and-provenance domain group
#### 9. integrity
Owns:
- checksum
- hash history
- corruption detection
- duplicate detection
- authenticity markers
- tamper-evidence linkage

Emits:
- DocumentIntegrityComputed
- DocumentIntegrityVerified
- DocumentIntegrityFailed
- DocumentDuplicateDetected
- DocumentCorruptionDetected

Consumed by:
- WhyceChain hooks
- governance/policy
- security/anti-abuse controls
- records and audit systems

#### 10. provenance
Owns:
- source attribution
- origin chain
- import provenance
- authorship/source references
- canonical lineage

Emits:
- DocumentProvenanceRecorded
- DocumentSourceLinked
- DocumentLineageUpdated
- DocumentProvenanceDisputed

Consumed by:
- governance
- rights/dispute systems
- external audit/reporting
- operational review workflows

---

### A4. lifecycle-and-change domain group
#### 11. version
Owns:
- document version chain
- major/minor versioning
- current version designation
- supersession/replacement
- rollback eligibility

Emits:
- DocumentVersionCreated
- DocumentVersionActivated
- DocumentVersionSuperseded
- DocumentVersionRolledBack
- DocumentVersionScheduled

Consumed by:
- publication
- governance/review
- preview/export
- operational consumers

#### 12. review
Owns:
- review lifecycle
- reviewer assignments
- approval/rejection states
- review notes/evidence hooks

Emits:
- DocumentReviewStarted
- DocumentReviewCompleted
- DocumentApproved
- DocumentRejected
- DocumentReturnedForAmendment

Consumed by:
- publication
- governance/policy
- operational approval flows

#### 13. publication
Owns:
- publish/unpublish
- draft-to-live transition
- schedule/embargo
- audience/channel visibility
- region availability markers

Emits:
- DocumentPublicationScheduled
- DocumentPublished
- DocumentUnpublished
- DocumentEmbargoApplied
- DocumentVisibilityChanged

Consumed by:
- catalog/discovery
- operational consumers
- distribution systems
- search/read models

---

### A5. representation domain group
#### 14. preview
Owns:
- preview generation
- thumbnail generation
- alternate lightweight representations
- text extraction representation

Emits:
- DocumentPreviewRequested
- DocumentPreviewGenerated
- DocumentPreviewFailed
- DocumentThumbnailGenerated
- DocumentTextExtracted

Consumed by:
- search/catalog
- UI surfaces
- moderation/review
- accessibility consumers

#### 15. export
Owns:
- derivative/exported document
- export format selection
- export lifecycle
- share/download-ready state

Emits:
- DocumentExportRequested
- DocumentExportGenerated
- DocumentExportFailed
- DocumentExportPublished

Consumed by:
- operational workflows
- external distribution
- user-facing delivery surfaces

---

### A6. governance domain group
#### 16. classification
Owns:
- sensitivity/confidentiality
- business criticality
- record/document class
- content-type governance categories

Emits:
- DocumentClassificationAssigned
- DocumentClassificationChanged
- DocumentSensitivityRaised
- DocumentSensitivityLowered

Consumed by:
- WHYCEPOLICY
- access/entitlement systems
- retention
- moderation/restriction consumers

#### 17. retention
Owns:
- retention policy binding
- legal hold
- disposal eligibility
- destruction authorization
- archive transfer

Emits:
- DocumentRetentionAssigned
- DocumentLegalHoldApplied
- DocumentLegalHoldReleased
- DocumentDisposalEligible
- DocumentDestroyed
- DocumentArchiveTransferred

Consumed by:
- governance/legal
- records management
- chain/audit systems

#### 18. moderation
Owns:
- flagged/restricted/quarantined state
- takedown/reinstatement
- abuse/dispute linkage
- safety review posture

Emits:
- DocumentFlagged
- DocumentRestricted
- DocumentQuarantined
- DocumentTakedownApplied
- DocumentReinstated

Consumed by:
- safety systems
- operational review
- policy and audit consumers

---

## B. media context

### B1. intake domain group
#### 19. ingest
Owns:
- media ingest request lifecycle
- source intake registration
- source media acceptance/rejection
- initial technical scan

Emits:
- MediaIngestRequested
- MediaIngestAccepted
- MediaIngestRejected
- MediaIngestStarted
- MediaIngestCompleted
- MediaIngestFailed

Consumed by:
- media.asset
- media.processing
- media.integrity
- streaming.stream

---

### B2. core-object domain group
#### 20. asset
Owns:
- canonical media asset
- audio/video/image identity
- source/master designation
- duration/resolution/container/codec metadata
- active state

Emits:
- MediaAssetCreated
- MediaAssetMetadataUpdated
- MediaAssetMasterDesignated
- MediaAssetArchived
- MediaAssetRestored

Consumed by:
- media.rendition
- media.package
- streaming.stream
- discovery/catalog consumers

#### 21. rendition
Owns:
- encoded variants
- bitrate/resolution variants
- device-specific or profile-specific outputs
- active/default rendition designation

Emits:
- MediaRenditionRequested
- MediaRenditionGenerated
- MediaRenditionFailed
- MediaRenditionActivated
- MediaRenditionDeprecated

Consumed by:
- streaming
- playback consumers
- catalog surfaces
- download/offline consumers

#### 22. artwork
Owns:
- poster/thumbnail/artwork truth
- artwork set selection
- default artwork selection

Emits:
- MediaArtworkAdded
- MediaArtworkRemoved
- MediaArtworkSetActivated
- MediaArtworkReplaced

Consumed by:
- discovery/catalog
- UI/presentation layers
- promotional consumers

#### 23. transcript
Owns:
- transcript truth
- transcript language variants
- transcript publication state
- transcript correction lifecycle

Emits:
- MediaTranscriptGenerated
- MediaTranscriptCorrected
- MediaTranscriptPublished
- MediaTranscriptWithdrawn

Consumed by:
- accessibility
- search/indexing
- learning/course consumers
- moderation/review consumers

#### 24. subtitle
Owns:
- subtitle/caption tracks
- language variants
- timing alignment
- active/default subtitle designation

Emits:
- MediaSubtitleAdded
- MediaSubtitleUpdated
- MediaSubtitlePublished
- MediaSubtitleWithdrawn

Consumed by:
- streaming
- accessibility
- playback consumers

#### 25. preview
Owns:
- teaser/clip/trailer/short preview truth
- primary preview selection
- preview publication state

Emits:
- MediaPreviewGenerated
- MediaPreviewPublished
- MediaPreviewWithdrawn
- MediaTrailerLinked

Consumed by:
- discovery/catalog
- recommendation/promo surfaces
- streaming preview delivery

---

### B3. technical-processing domain group
#### 26. processing
Owns:
- transcode lifecycle
- packaging readiness
- thumbnail generation lifecycle
- repair/reprocess state

Emits:
- MediaProcessingQueued
- MediaTranscodeStarted
- MediaTranscodeCompleted
- MediaTranscodeFailed
- MediaReprocessingRequested
- MediaPackagingCompleted

Consumed by:
- streaming
- operational monitoring
- quality control consumers

#### 27. quality
Owns:
- quality-control review
- technical issue state
- pass/fail readiness
- operator technical approval

Emits:
- MediaQualityCheckPassed
- MediaQualityCheckFailed
- MediaIssueDetected
- MediaIssueResolved

Consumed by:
- publication
- streaming
- operational support systems

#### 28. integrity
Owns:
- hash/fingerprint
- duplicate detection
- corruption detection
- incomplete upload detection
- media authenticity markers

Emits:
- MediaIntegrityComputed
- MediaIntegrityVerified
- MediaIntegrityFailed
- MediaDuplicateDetected
- MediaCorruptionDetected

Consumed by:
- governance
- WhyceChain hooks
- anti-abuse and review consumers

---

### B4. composition-and-catalog domain group
#### 29. package
Owns:
- composite media package
- playlist/sequence membership
- series/course/channel asset grouping
- asset order and package structure

Emits:
- MediaPackageCreated
- MediaPackageItemAdded
- MediaPackageItemRemoved
- MediaPackageReordered
- MediaPackagePublished

Consumed by:
- streaming
- catalog/discovery
- learning/course consumers
- subscription/catalog consumers

#### 30. sequence
Owns:
- ordered play sequence
- episode/lesson/item ordering
- continuity metadata

Emits:
- MediaSequenceCreated
- MediaSequenceItemAdded
- MediaSequenceReordered
- MediaSequencePublished

Consumed by:
- streaming replay/order consumers
- course/series consumers
- autoplay/binge consumers

#### 31. metadata
Owns:
- canonical media metadata
- localized titles/descriptions
- tags/genre/category
- cast/speaker/instructor attribution
- warnings and content rating

Emits:
- MediaMetadataAssigned
- MediaMetadataLocalized
- MediaMetadataUpdated
- MediaRatingAssigned
- MediaWarningTagged

Consumed by:
- search/catalog
- recommendation consumers
- moderation/review
- rights/publication

---

### B5. rights-and-publication domain group
#### 32. rights
Owns:
- ownership state
- license binding
- usage rights
- territory/platform restriction metadata
- expiry/embargo windows
- dispute state

Emits:
- MediaRightsAssigned
- MediaLicenseBound
- MediaRightsRestricted
- MediaRightsExpired
- MediaRightsDisputed
- MediaRightsResolved

Consumed by:
- streaming availability
- entitlement/policy consumers
- publication control
- audit/compliance systems

#### 33. publication
Owns:
- draft/review/approved/scheduled/live/withdrawn state
- geo-limited release
- catalog inclusion
- audience visibility markers

Emits:
- MediaPublicationScheduled
- MediaPublished
- MediaUnpublished
- MediaWithdrawn
- MediaVisibilityChanged

Consumed by:
- streaming
- catalog/discovery
- promotional consumers
- operational consumer systems

---

### B6. safety-and-governance domain group
#### 34. moderation
Owns:
- flags/restrictions/quarantine
- age restriction
- copyright claim posture
- takedown/appeal/reinstatement

Emits:
- MediaFlagged
- MediaAgeRestricted
- MediaRestricted
- MediaTakedownApplied
- MediaAppealOpened
- MediaReinstated

Consumed by:
- streaming
- catalog/discovery
- policy and audit consumers

#### 35. accessibility
Owns:
- accessibility completeness state
- descriptive audio linkage
- alternate audio accessibility variants
- subtitle/caption availability readiness

Emits:
- MediaAccessibilityCompleted
- MediaAccessibilityIncomplete
- MediaAccessibilityUpdated

Consumed by:
- streaming
- publication
- compliance/reporting consumers

---

## C. streaming context

### C1. stream-core domain group
#### 36. stream
Owns:
- canonical stream object
- on-demand/live classification
- asset/package binding
- readiness state
- active/inactive/degraded/failed state

Emits:
- StreamCreated
- StreamProvisioned
- StreamActivated
- StreamDegraded
- StreamFailed
- StreamRecovered
- StreamRetired

Consumed by:
- playback/session consumers
- catalog/discovery
- operational observability systems

#### 37. manifest
Owns:
- stream manifest truth
- manifest generation lifecycle
- variant manifest linkage
- manifest freshness state

Emits:
- StreamManifestRequested
- StreamManifestGenerated
- StreamManifestUpdated
- StreamManifestFailed
- StreamManifestExpired

Consumed by:
- playback clients
- operational delivery systems
- health monitoring consumers

#### 38. availability
Owns:
- stream start/end windows
- region/jurisdiction/device availability
- publication visibility
- temporary suspension/emergency stop

Emits:
- StreamAvailabilityScheduled
- StreamAvailabilityOpened
- StreamAvailabilityClosed
- StreamAvailabilityRestricted
- StreamEmergencyStopped

Consumed by:
- entitlement/access consumers
- policy/governance
- playback/session systems
- catalog/discovery

---

### C2. playback-and-consumption domain group
#### 39. session
Owns:
- playback session lifecycle
- session start/close
- heartbeat
- device linkage
- concurrent session tracking
- playback failure state

Emits:
- PlaybackSessionOpened
- PlaybackSessionHeartbeatRecorded
- PlaybackSessionClosed
- PlaybackSessionFailed
- PlaybackSessionConcurrencyExceeded

Consumed by:
- analytics/projections
- entitlement/revalidation hooks
- operational monitoring consumers

#### 40. progress
Owns:
- resume point
- last position
- watch duration
- completion percentage
- completion state
- abandonment/drop-off markers

Emits:
- PlaybackProgressRecorded
- PlaybackResumePointUpdated
- PlaybackCompleted
- PlaybackAbandoned

Consumed by:
- learning progression consumers
- recommendation/engagement consumers
- autoplay/sequence consumers

#### 41. replay
Owns:
- replay state
- last watched timestamp
- replay publication posture
- replay replacement lineage

Emits:
- ReplayPublished
- ReplayWithdrawn
- ReplayReplaced
- ReplayExpired

Consumed by:
- catalog/discovery
- archive consumers
- watch-history consumers

---

### C3. live-streaming domain group
#### 42. broadcast
Owns:
- live broadcast lifecycle
- broadcast start/pause/resume/stop
- live visibility
- interruption/failover
- operator control state

Emits:
- LiveBroadcastCreated
- LiveBroadcastStarted
- LiveBroadcastPaused
- LiveBroadcastResumed
- LiveBroadcastStopped
- LiveBroadcastInterrupted
- LiveBroadcastRecovered

Consumed by:
- live catalog/discovery
- archive generation
- operational support consumers
- moderation/review consumers

#### 43. ingest-session
Owns:
- live ingest source/session
- ingest health and registration
- ingest endpoint/source linkage
- ingest failure state

Emits:
- LiveIngestSessionOpened
- LiveIngestHealthDegraded
- LiveIngestSessionFailed
- LiveIngestSessionClosed

Consumed by:
- broadcast
- operational observability
- incident response consumers

#### 44. archive
Owns:
- live-to-archive conversion
- replay/archive object linkage
- archive retention and quality review
- highlight clip derivation linkage

Emits:
- LiveArchiveCreated
- LiveArchivePublished
- LiveArchiveTrimmed
- LiveArchiveExpired
- LiveHighlightDerived

Consumed by:
- replay
- media.asset/package
- catalog/discovery
- retention consumers

---

### C4. delivery-governance domain group
#### 45. entitlement-hook
Owns:
- stream-side entitlement validation hook state
- subscription/access readiness markers
- revalidation triggers

Emits:
- StreamEntitlementValidated
- StreamEntitlementRejected
- StreamRevalidationRequested

Consumed by:
- economic/access systems
- playback session control
- operational audit consumers

#### 46. moderation
Owns:
- stream review/restriction state
- live moderation hold
- unsafe shutdown
- emergency takedown linkage

Emits:
- StreamFlagged
- StreamModerationHoldApplied
- StreamRestricted
- StreamShutdownApplied
- StreamReinstated

Consumed by:
- broadcast/session
- catalog/discovery
- policy/audit consumers

#### 47. observability
Owns:
- technical stream health truth
- latency/degradation/dropout markers
- recovery events
- SLA/SLO evidence hooks

Emits:
- StreamHealthDegraded
- StreamLatencyExceeded
- StreamDropoutDetected
- StreamRecovered
- StreamSlaBreached

Consumed by:
- operations/incident systems
- support tooling
- governance/reporting consumers

---

## D. shared content-system cross-context domains

### D1. relationship domain group
#### 48. relationship
Owns:
- parent-child links across content objects
- related-content graph
- derivative/source linkage
- archive/live/replay linkage
- alternate-version linkage

Emits:
- ContentRelationshipCreated
- ContentRelationshipRemoved
- ContentRelationshipReclassified

Consumed by:
- catalog/discovery
- operational systems
- governance and audit consumers

### D2. localization domain group
#### 49. localization
Owns:
- localized titles/descriptions
- localized metadata variants
- region-language publishing variants
- localized representation linkage

Emits:
- ContentLocalized
- ContentLocalizationUpdated
- ContentLocalizationWithdrawn

Consumed by:
- catalog/discovery
- publication/availability
- playback/presentation consumers

### D3. provenance-evidence domain group
#### 50. evidence
Owns:
- operator action evidence
- lifecycle evidence references
- provenance evidence linkage
- chain-anchor hooks
- policy decision linkage hooks

Emits:
- ContentEvidenceRecorded
- ContentPolicyDecisionLinked
- ContentChainAnchorLinked
- ContentOperatorActionRecorded

Consumed by:
- WhyceChain
- WHYCEPOLICY
- governance/audit systems
- dispute/review consumers

---

## E. concise ownership split

### document context owns
- file/document truth
- records/templates/bundles
- versions/reviews/publication for documents
- retention and document governance
- document previews/exports

### media context owns
- durable media asset truth
- renditions/transcripts/subtitles/previews
- metadata/catalog truth for media
- rights/publication/moderation/accessibility
- technical processing and package composition

### streaming context owns
- stream delivery truth
- playback/live/archive/replay truth
- stream availability/session/progress truth
- live ingest/broadcast control state
- stream moderation/entitlement hooks/observability truth