Good.
We can define **Phase 2.6 — Content-System Implementation** as the next Phase 2 implementation block focused on the full **content upload, storage, processing, delivery, and streaming foundation**.

The purpose of Phase 2.6 should be:

**implement the content-system canonically across domain, ingestion, storage, processing, delivery, streaming, policy, messaging, projections, and platform exposure, at the same implementation standard as economic and structural.**

A clean implementation-topic plan for **Phase 2.6** is below.

**Phase 2.6 — Content-System Implementation Topics**

**1. Content-system canonical definition**

* content-system canonical scope
* content-system boundaries
* content-system purpose inside Whycespace
* relationship between content-system and structural-system
* relationship between content-system and identity
* relationship between content-system and policy
* relationship between content-system and economic controls where applicable
* canonical content terminology lock
* content asset classification rules
* content lifecycle classification rules

**2. Content taxonomy and domain map**

* content classification map
* content context map
* content domain-group map
* content domain map
* upload domain map
* storage domain map
* asset domain map
* processing domain map
* streaming domain map
* delivery domain map
* naming normalization
* namespace and route normalization
* topic naming normalization
* content ownership boundaries

**3. Core content model**

* content asset model
* content object model
* content file model
* content media model
* content stream model
* content variant model
* content manifest model
* content metadata model
* content ownership model
* content access model

**4. Content identity and reference model**

* deterministic content IDs
* upload IDs
* asset IDs
* object IDs
* stream IDs
* manifest IDs
* content reference model
* owner reference rules
* source reference rules
* structural linkage rules
* immutable content identity fields
* mutable metadata fields

**5. Core content domains implementation**

* upload domain implementation
* asset domain implementation
* object domain implementation
* media domain implementation
* manifest domain implementation
* processing domain implementation
* delivery domain implementation
* streaming domain implementation
* content-access domain implementation
* content-state domain implementation

**6. Content aggregate design**

* aggregate root definition per content domain
* content entities
* content value objects
* content metadata objects
* content state model
* content status transitions
* content invariants
* content specifications
* content errors
* content events
* content command model
* content query model

**7. Upload and ingestion model**

* upload session model
* single-part upload handling
* multipart upload handling
* resumable upload model
* upload completion rules
* upload cancellation rules
* upload expiry rules
* upload integrity verification
* upload size limits
* upload content-type validation
* upload checksum validation
* upload deduplication strategy

**8. Content storage model**

* object storage abstraction
* file persistence boundaries
* object key strategy
* bucket/container classification
* raw object storage model
* processed object storage model
* archive storage model
* temporary upload storage model
* retention-aware storage rules
* deletion and tombstone rules
* storage immutability rules where required

**9. Content processing model**

* content processing pipeline
* metadata extraction
* media inspection
* transcoding model
* thumbnail generation
* preview generation
* segmentation model for streaming
* packaging model
* format normalization
* processing retry rules
* failed processing handling
* processing compensation boundaries

**10. Streaming model**

* stream asset model
* stream manifest generation
* stream segment generation
* adaptive bitrate support model
* live stream support model if in scope
* on-demand stream support model
* stream publication rules
* stream expiry rules
* stream access gating
* stream continuity and availability rules
* playback token model where needed

**11. Delivery model**

* content delivery routes
* signed delivery model
* direct object access restrictions
* CDN integration boundary
* edge delivery control model
* preview delivery model
* download delivery model
* streaming delivery model
* delivery authorization model
* delivery revocation model

**12. Content ownership and linkage**

* user-to-content ownership rules
* structure-to-content ownership rules
* SPV/brand/cluster content association rules
* creator and uploader distinction
* custodian rules
* transfer-of-ownership rules
* delegated access rules
* content isolation boundaries
* cross-tenant leakage prevention
* canonical ownership lookup rules

**13. Content lifecycle modeling**

* upload lifecycle
* asset creation lifecycle
* processing lifecycle
* publication lifecycle
* streaming-ready lifecycle
* suspension lifecycle
* archival lifecycle
* deletion lifecycle
* restoration lifecycle
* retention lifecycle
* legal-hold or restricted-state lifecycle if needed

**14. Content business invariants**

* no-orphan-object invariant
* no-publish-before-processing-ready invariant
* ownership-required invariant
* checksum integrity invariant
* allowed-format invariant
* allowed-size invariant
* no-invalid-stream-publication invariant
* retention and deletion invariant
* no-direct-access-bypass invariant
* content-state transition invariants

**15. Content policy model**

* content policy ID definitions
* policy package layout
* upload allow/deny policy
* format and type restriction policy
* ownership and tenant policy
* processing authorization policy
* publication policy
* streaming access policy
* download access policy
* delete/archive policy
* policy simulation coverage
* policy decision audit coverage

**16. Content runtime integration**

* command routing into content domains
* middleware enforcement for upload commands
* middleware enforcement for publication commands
* policy evaluation integration
* authorization integration
* idempotency integration
* execution guard integration
* deterministic command execution
* replay-safe content execution
* runtime context propagation
* correlation and causation tracing

**17. Content engine implementation**

* upload application handlers
* asset creation handlers
* processing handlers
* storage coordination services
* manifest generation services
* streaming preparation services
* delivery authorization services
* access-token services where needed
* metadata services
* integrity verification services

**18. Persistence and event sourcing**

* content event stream definitions
* event versioning rules
* asset rehydration correctness
* upload-state persistence
* processing-state persistence
* stream-state persistence
* optimistic concurrency rules
* deterministic event serialization
* replay determinism checks
* content persistence auditability

**19. Object storage and infrastructure integration**

* MinIO or object storage adapter alignment
* bucket provisioning rules
* object naming conventions
* metadata persistence boundaries
* pre-signed upload/download integration
* multipart upload coordination
* object existence verification
* storage failure handling
* storage retry rules
* storage audit logging

**20. Messaging and Kafka**

* content topic map
* upload command topics
* asset event topics
* processing topics
* retry topics
* deadletter topics
* topic naming compliance
* event contract registration
* header contract compliance
* outbox integration
* publish/retry/DLQ behavior

**21. Projections and read models**

* content asset read models
* upload status read models
* processing status read models
* streaming-ready read models
* manifest lookup read models
* content ownership read models
* content access read models
* delivery status read models
* projection reducers
* projection replay and catch-up validation

**22. Platform API exposure**

* upload session endpoints
* upload completion endpoints
* asset registration endpoints
* content metadata endpoints
* processing status endpoints
* publish/unpublish endpoints
* streaming access endpoints
* download access endpoints
* content lookup/search endpoints
* administrative control endpoints
* canonical API route mapping

**23. Streaming API and playback surface**

* playback manifest endpoints
* segment access endpoints
* tokenized playback endpoints
* preview playback endpoints
* stream status endpoints
* access revocation behavior
* playback authorization checks
* playback observability hooks
* rate and abuse control hooks
* stream exposure boundaries

**24. Content observability and evidence**

* trace coverage for uploads
* trace coverage for processing
* trace coverage for streaming
* upload failure signals
* processing failure signals
* manifest generation signals
* delivery failure signals
* projection lag signals
* object storage health signals
* audit and evidence completeness

**25. Content security and access control**

* MIME/type spoof protection
* file extension trust restrictions
* malware scanning boundary if required
* unsafe content quarantine model
* signed URL restrictions
* expiry enforcement
* tenant isolation enforcement
* role-based access checks
* policy-based access checks
* abuse and hotlink prevention boundaries

**26. Content integration boundaries**

* structural-system linkage rules
* identity-system linkage rules
* policy-system linkage rules
* economic linkage rules where monetized content exists
* workflow boundary rules
* notification boundary hooks if needed
* external CDN boundary rules
* external transcoder boundary rules if used
* no-cross-domain-drift rules
* content-to-platform consistency rules

**27. Content test and certification topics**

* aggregate and invariant tests
* replay determinism tests
* upload API tests
* multipart/resumable upload tests
* checksum verification tests
* processing pipeline tests
* manifest generation tests
* stream access tests
* projection correctness tests
* end-to-end regression pack

**28. Content resilience validation**

* interrupted upload recovery
* resumable upload recovery
* duplicate upload command resistance
* object storage outage handling
* Kafka interruption recovery
* projection recovery tests
* processing retry and DLQ tests
* partial publication failure handling
* stream readiness recovery
* consistency after restart

**29. Content documentation and anti-drift**

* content canonical README set
* domain map documentation
* upload/storage/streaming catalog
* command/event catalog
* policy catalog
* projection catalog
* API contract documentation
* guard updates for content rules
* audit updates for content rules
* completion evidence pack

**30. Phase 2.6 completion criteria**

* content domains implemented canonically
* upload flow verified
* storage integration verified
* processing pipeline verified
* streaming foundation verified
* policy enforcement verified
* runtime integration verified
* messaging verified
* projections verified
* API verified
* regression pack passing
* completion evidence produced

For execution control, I would run it in this order:

**Batch A — Foundation**

* canonical definition
* taxonomy and domain map
* core content model
* identity and reference model

**Batch B — Upload and storage**

* upload and ingestion model
* storage model
* ownership and linkage
* lifecycle modeling
* business invariants

**Batch C — Processing and streaming**

* processing model
* streaming model
* delivery model
* security and access control

**Batch D — Enforcement and integration**

* policy model
* runtime integration
* engine implementation
* persistence and event sourcing
* object storage integration
* messaging and Kafka

**Batch E — Read side and proof**

* projections and read models
* platform API exposure
* streaming API surface
* observability and evidence
* integration boundaries
* tests
* resilience validation
* documentation and anti-drift
* completion criteria evidence

The key scope rule should be:

**Phase 2.6 is the content-system implementation foundation for upload and streaming.**
It is not broad future media expansion, and it should stay limited to the canonical implementation pass required inside Phase 2.

A good one-line canonical description for it would be:

**Phase 2.6 — implement the content-system end to end for content upload, storage, processing, delivery, and streaming readiness.**

The best next move is to split this into **content subdomains** so we lock the actual domain map before implementation starts.
