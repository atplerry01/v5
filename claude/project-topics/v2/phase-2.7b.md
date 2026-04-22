Proceeding with the **governance context full D2 build plan** for **Phase 2.7B**.

This is the right next artifact because the governance context under `decision-system` holds the foundational enforcement bodies for WHYCEPOLICY — the Guardian and Quorum domains are the two primary policy-decision safeguards. Without them at D2, no multi-party approval, no policy-release gate, and no guardian-backed decision integrity can be wired into the T0U runtime.

Use this as the working plan.

**Phase 2.7B — Governance / Guardian / Quorum Full D2 Implementation Plan**

**Controlling statement:**
Phase 2.7B implements the full `decision-system/governance` context to D2 across all bounded contexts, with primary depth on the Guardian and Quorum domains as the constitutional anchors of policy decision integrity — then wires the complete set through engine, runtime, policy, persistence, messaging, projections, API, and cross-domain integration.

---

**2.7B.0 Foundation and control topics**

* governance context canonical scope lock
* governance BC inventory confirmation and activation-level audit
* Guardian domain role in policy lifecycle (oversight gating)
* Quorum domain role in policy lifecycle (threshold enforcement)
* governance-to-constitutional-system dependency map
* governance-to-trust-system dependency map
* governance-to-economic-system dependency map
* governance event catalog baseline
* governance policy catalog baseline
* governance command catalog baseline
* completion-gate definition for Phase 2.7B

**Current governance BC activation state:**

| BC | Current Level | Target |
|----|--------------|--------|
| `access-review` | D1 (no aggregate) | D2 |
| `appeal` | D1 (no aggregate) | D2 |
| `approval` | D1 (stub aggregate) | D2 |
| `authority` | D1 (no aggregate) | D2 |
| `charter` | D1 (no aggregate) | D2 |
| `cluster-decision` | D1 (no aggregate) | D2 |
| `committee` | D1 (no aggregate) | D2 |
| `compliance-review` | D1 (no aggregate) | D2 |
| `delegation` | D1 (no aggregate) | D2 |
| `dispute` | D1 (no aggregate) | D2 |
| `exception` | D1 (no aggregate) | D2 |
| `governance-cycle` | D1 (no aggregate) | D2 |
| `governance-record` | D1 (no aggregate) | D2 |
| `guardian` | D1 (stub aggregate) | D2 |
| `mandate` | D1 (no aggregate) | D2 |
| `proposal` | D1 (no aggregate) | D2 |
| `quorum` | D1 (stub aggregate) | D2 |

---

**2.7B.1 Guardian Domain — Full D2**

Purpose:
* govern oversight actors responsible for protecting decision integrity
* track guardian appointment, scope, oversight actions, conflicts, and accountability
* ensure every high-impact governance decision has a designated, policy-authorized guardian

Implementation topics — aggregate:

* `GuardianAggregate` full business logic implementation
* factory method: `GuardianAggregate.Appoint(guardianId, identityId, scope, mandateRef, clock)` — raises `GuardianAppointedEvent`
* `Activate()` — validates scope completeness, raises `GuardianActivatedEvent`
* `RecordOversight(decisionRef, outcome, clock)` — raises `GuardianOversightRecordedEvent`
* `DeclareConflict(conflictRef, clock)` — raises `GuardianConflictDeclaredEvent`
* `Reassign(newScope, clock)` — raises `GuardianReassignedEvent`
* `Retire(reason, clock)` — raises `GuardianRetiredEvent`
* `Archive(clock)` — raises `GuardianArchivedEvent`
* `UpdateScope(scope, clock)` — raises `GuardianScopeUpdatedEvent`
* lifecycle: `Unassigned → Appointed → Active → OversightRecorded → Reassigned / Retired → Archived`
* version-based optimistic concurrency on all mutations

Implementation topics — value objects:

* `GuardianId` — deterministic id, no `Guid.NewGuid()`
* `GuardianScope` — immutable record: classification, context domains, decision types covered
* `OversightRecord` — immutable: decisionRef, outcome (Approved/Denied/Deferred), recordedAt
* `GuardianStatus` — enum: `Unassigned | Appointed | Active | ConflictFlagged | Reassigned | Retired | Archived`
* `ConflictDeclaration` — immutable: conflictRef, reason, declaredAt

Implementation topics — events (full payload, past-tense, deterministic):

* `GuardianAppointedEvent` — guardianId, identityId, scope, mandateRef, appointedAt
* `GuardianActivatedEvent` — guardianId, activatedAt
* `GuardianOversightRecordedEvent` — guardianId, decisionRef, outcome, recordedAt
* `GuardianConflictDeclaredEvent` — guardianId, conflictRef, reason, declaredAt
* `GuardianReassignedEvent` — guardianId, previousScope, newScope, reassignedAt
* `GuardianRetiredEvent` — guardianId, reason, retiredAt
* `GuardianArchivedEvent` — guardianId, archivedAt
* `GuardianScopeUpdatedEvent` — guardianId, previousScope, newScope, updatedAt

Implementation topics — specifications:

* `GuardianScopeDefinedSpecification` — scope must not be empty before activation
* `GuardianActiveSpecification` — guard cannot oversee unless status is Active
* `GuardianConflictFreeSpecification` — no active conflict declaration before oversight action
* `GuardianScopeCoversDecisionSpecification` — decision type must fall within guardian scope

Implementation topics — errors:

* `GuardianErrors.ScopeNotDefined`
* `GuardianErrors.GuardianNotActive`
* `GuardianErrors.ConflictOfInterestDeclared`
* `GuardianErrors.DecisionOutsideScope`
* `GuardianErrors.AlreadyRetired`
* `GuardianErrors.AlreadyArchived`
* `GuardianErrors.CannotReassignArchivedGuardian`

Implementation topics — domain service:

* `GuardianService.ValidateOversightEligibility(guardian, decisionRef)` — pure, no side effects
* `GuardianService.ComputeGuardianStatus(events)` — deterministic from event history

Implementation topics — invariants:

* a guardian must have a defined scope before activation
* guardian oversight actions must be recorded with full decision reference
* guardian assignments must not create conflicts of interest
* a retired or archived guardian cannot take new oversight actions
* guardian scope changes must not retroactively alter recorded oversights
* all guardian lifecycle mutations require policy authorization

Completion proof:

* guardian appointment lifecycle end-to-end
* conflict-of-interest path rejects oversight action
* retired guardian path blocks new actions
* deterministic event replay produces identical aggregate state
* scope validation prevents out-of-bounds oversight
* all events carry correct payload fields
* `GuardianAggregate` replay-safe round-trip passes

---

**2.7B.2 Quorum Domain — Full D2**

Purpose:
* enforce multi-party participation thresholds for governance decisions
* validate quorum satisfaction before any governance action becomes binding
* track quorum verification, member participation, and outcome recording

Implementation topics — aggregate:

* `QuorumAggregate` full business logic implementation
* factory method: `QuorumAggregate.Define(quorumId, body, threshold, members, clock)` — raises `QuorumDefinedEvent`
* `AddMember(memberId, weight, clock)` — raises `QuorumMemberAddedEvent`
* `RemoveMember(memberId, reason, clock)` — raises `QuorumMemberRemovedEvent`
* `StartVerification(decisionRef, clock)` — raises `QuorumVerificationStartedEvent`
* `RecordVote(memberId, vote, clock)` — raises `QuorumVoteRecordedEvent`
* `RecordAbstention(memberId, clock)` — raises `QuorumAbstentionRecordedEvent`
* `EvaluateQuorum(clock)` — deterministic evaluation: raises `QuorumMetEvent` or `QuorumNotMetEvent`
* `Record(decisionRef, result, clock)` — raises `QuorumRecordedEvent`
* `Amend(newThreshold, rationale, clock)` — raises `QuorumAmendedEvent` (cannot amend after recorded for a decision)
* `Expire(clock)` — raises `QuorumExpiredEvent`
* lifecycle: `Defined → Verified → Met / NotMet → Recorded` with `Expired` as a terminal from any non-recorded state
* version-based optimistic concurrency on all mutations

Implementation topics — value objects:

* `QuorumId` — deterministic id
* `QuorumThreshold` — immutable: type (`Absolute | Percentage | Supermajority | Unanimous`), value
* `QuorumMember` — immutable: memberId, weight (defaulting to 1 for unweighted)
* `VoteTally` — immutable: yesCount, noCount, abstentionCount, thresholdValue, thresholdType, met (bool)
* `QuorumStatus` — enum: `Defined | VerificationPending | InProgress | Met | NotMet | Recorded | Expired | Amended`
* `QuorumRecord` — immutable: decisionRef, result, recordedAt, tallySnapshot

Implementation topics — events (full payload, deterministic):

* `QuorumDefinedEvent` — quorumId, body, threshold, members, definedAt
* `QuorumMemberAddedEvent` — quorumId, memberId, weight, addedAt
* `QuorumMemberRemovedEvent` — quorumId, memberId, reason, removedAt
* `QuorumVerificationStartedEvent` — quorumId, decisionRef, startedAt
* `QuorumVoteRecordedEvent` — quorumId, memberId, vote, recordedAt
* `QuorumAbstentionRecordedEvent` — quorumId, memberId, recordedAt
* `QuorumMetEvent` — quorumId, decisionRef, tally, evaluatedAt
* `QuorumNotMetEvent` — quorumId, decisionRef, tally, evaluatedAt
* `QuorumRecordedEvent` — quorumId, decisionRef, result, recordedAt
* `QuorumAmendedEvent` — quorumId, previousThreshold, newThreshold, rationale, amendedAt
* `QuorumExpiredEvent` — quorumId, expiredAt

Implementation topics — specifications:

* `QuorumThresholdMetSpecification` — deterministic evaluation: tally satisfies threshold type and value
* `QuorumMemberEligibleSpecification` — member must be in active membership list before voting
* `QuorumVerificationActiveSpecification` — verification must be in InProgress before votes can be recorded
* `QuorumNotYetRecordedSpecification` — threshold changes forbidden after recording for a decision
* `QuorumMemberParticipationCompleteSpecification` — all members have participated before final evaluation

Implementation topics — errors:

* `QuorumErrors.ThresholdNotDefined`
* `QuorumErrors.MemberNotEligible`
* `QuorumErrors.VerificationNotActive`
* `QuorumErrors.AlreadyRecorded`
* `QuorumErrors.CannotAmendAfterRecord`
* `QuorumErrors.DuplicateVote`
* `QuorumErrors.QuorumExpired`
* `QuorumErrors.ThresholdTypeNotSupported`

Implementation topics — domain service:

* `QuorumService.EvaluateThreshold(tally, threshold)` — pure function, deterministic, no I/O
* `QuorumService.ComputeParticipationRate(tally, memberCount)` — pure
* `QuorumService.IsQuorumMet(tally, threshold)` — canonical boolean, called by aggregate before event emission

Implementation topics — invariants:

* quorum thresholds must be defined before verification can start
* only active members may cast votes
* each member may vote exactly once per verification session (no duplicate votes)
* quorum cannot be amended after it has been recorded for a decision
* quorum evaluation is deterministic: same tally + threshold = same result on replay
* all quorum lifecycle mutations require policy authorization
* expired quorum cannot produce a valid decision record

Completion proof:

* quorum defined and verified end-to-end with Met and NotMet paths
* duplicate vote rejected
* post-record amendment rejected
* expired quorum blocks recording
* deterministic evaluation: two replay runs produce identical quorum evaluation outcome
* all events carry correct payload fields
* `QuorumAggregate` replay-safe round-trip passes

---

**2.7B.3 Approval Domain — Full D2**

Purpose: govern multi-step approval chains for governance actions

Implementation topics:

* `ApprovalAggregate` — full business logic replacing stub
* approval request lifecycle: `Requested → UnderReview → Approved / Rejected / Deferred → Closed`
* approval chain model (sequential and parallel approvers)
* approver assignment rules
* approval deadline rules
* escalation path on deadline breach
* approval events: `ApprovalRequestedEvent`, `ApprovalGrantedEvent`, `ApprovalDeniedEvent`, `ApprovalDeferredEvent`, `ApprovalEscalatedEvent`, `ApprovalClosedEvent`
* value objects: `ApprovalId`, `ApprovalChain`, `ApprovalStep`, `ApproverRef`, `ApprovalDeadline`, `ApprovalOutcome`
* invariants: no approval without declared chain; deadline must precede close; denied chain cannot be re-approved without re-request
* policy binding: approval request command is policy-gated

Completion proof: full chain approval lifecycle; escalation triggers correctly; deterministic replay

---

**2.7B.4 Proposal Domain — Full D2**

Purpose: manage governance proposals from submission through acceptance or rejection

Implementation topics:

* `ProposalAggregate` — full business logic
* lifecycle: `Drafted → Submitted → UnderReview → Accepted / Rejected / Withdrawn → Archived`
* proposal classification (policy proposal, amendment, revocation, structural)
* affected domain and system declaration
* rationale and justification capture
* reviewer assignment
* simulation requirement flag
* proposal events: `ProposalDraftedEvent`, `ProposalSubmittedEvent`, `ProposalAcceptedEvent`, `ProposalRejectedEvent`, `ProposalWithdrawnEvent`, `ProposalArchivedEvent`
* value objects: `ProposalId`, `ProposalClassification`, `AffectedDomainRef`, `ProposalRationale`, `SimulationRequirement`
* invariants: proposals must have classification and affected domain before submission; withdrawn proposals cannot be re-submitted without new draft

Completion proof: proposal lifecycle end-to-end; classification validation; simulation flag respected

---

**2.7B.5 Committee Domain — Full D2**

Purpose: represent governance decision-making bodies with defined membership and mandate

Implementation topics:

* `CommitteeAggregate` — full business logic
* lifecycle: `Defined → Active → Suspended → Dissolved`
* membership management (add/remove member, role assignment within committee)
* mandate linkage
* quorum reference linkage
* committee events: `CommitteeDefinedEvent`, `CommitteeActivatedEvent`, `MemberJoinedCommitteeEvent`, `MemberLeftCommitteeEvent`, `CommitteeSuspendedEvent`, `CommitteeDissolvedEvent`
* value objects: `CommitteeId`, `CommitteeMember`, `CommitteeRole`, `CommitteeMandate`
* invariants: committee must have at least one member before activation; dissolved committee cannot be reactivated

Completion proof: committee lifecycle; membership management; quorum linkage correctness

---

**2.7B.6 Authority Domain — Full D2**

Purpose: declare authoritative bodies and their governance scope

Implementation topics:

* `AuthorityAggregate` — full business logic
* lifecycle: `Declared → Active → Revoked`
* scope of authority declaration (classification, contexts, action types)
* authority delegation model
* authority conflict detection invariant
* authority events: `AuthorityDeclaredEvent`, `AuthorityActivatedEvent`, `AuthorityRevokedEvent`, `AuthorityScopeUpdatedEvent`
* value objects: `AuthorityId`, `AuthorityScope`, `AuthorityType`
* invariants: authority must have non-empty scope; overlapping authority scopes require explicit resolution

Completion proof: authority scope validation; revocation path; conflict invariant fires correctly

---

**2.7B.7 Charter Domain — Full D2**

Purpose: define the governing rules and mandate of a governance body

Implementation topics:

* `CharterAggregate` — full business logic
* lifecycle: `Drafted → Ratified → Amended → Revoked`
* charter ratification requirements
* amendment flow with version tracking
* revocation rules
* charter events: `CharterDraftedEvent`, `CharterRatifiedEvent`, `CharterAmendedEvent`, `CharterRevokedEvent`
* value objects: `CharterId`, `CharterVersion`, `CharterClause`, `RatificationRecord`
* invariants: charter must be ratified before governing body can activate; amendments require ratification quorum

Completion proof: charter lifecycle; amendment version tracking; ratification requirement enforced

---

**2.7B.8 Delegation Domain — Full D2**

Purpose: govern the transfer of authority from one actor to another within defined scope

Implementation topics:

* `DelegationAggregate` — full business logic
* lifecycle: `Requested → Active → Suspended / Revoked → Expired`
* delegation scope (classification, contexts, action types)
* delegation chain limits (prevent unbounded re-delegation)
* expiry rules
* delegation events: `DelegationGrantedEvent`, `DelegationActivatedEvent`, `DelegationSuspendedEvent`, `DelegationRevokedEvent`, `DelegationExpiredEvent`
* value objects: `DelegationId`, `DelegationScope`, `DelegationChainDepth`, `DelegationExpiry`
* invariants: delegation scope cannot exceed delegator's scope; delegation chain depth is bounded; expired delegation cannot be reactivated

Completion proof: delegation grant lifecycle; scope constraint enforced; chain depth limit fires

---

**2.7B.9 Mandate Domain — Full D2**

Purpose: formalize binding mandates given to governance actors and bodies

Implementation topics:

* `MandateAggregate` — full business logic
* lifecycle: `Issued → Active → Fulfilled / Revoked / Expired`
* mandate scope and obligations
* mandate evidence recording
* fulfilment verification
* mandate events: `MandateIssuedEvent`, `MandateActivatedEvent`, `MandateFulfilledEvent`, `MandateRevokedEvent`, `MandateExpiredEvent`, `MandateEvidenceRecordedEvent`
* value objects: `MandateId`, `MandateScope`, `MandateObligation`, `MandateFulfilmentRecord`, `MandateExpiry`
* invariants: mandate obligations must be defined at issuance; expired mandate cannot be fulfilled; revoked mandate cannot produce evidence records

Completion proof: mandate lifecycle; fulfilment evidence linkage; expiry path correctness

---

**2.7B.10 Governance Cycle Domain — Full D2**

Purpose: track recurring governance review and reporting cycles

Implementation topics:

* `GovernanceCycleAggregate` — full business logic
* lifecycle: `Scheduled → Open → Closed → Archived`
* cycle period (start/end)
* items under review linkage
* cycle closure requirements (all items resolved)
* cycle events: `GovernanceCycleScheduledEvent`, `GovernanceCycleOpenedEvent`, `GovernanceCycleClosedEvent`, `GovernanceCycleArchivedEvent`, `CycleItemAddedEvent`, `CycleItemResolvedEvent`
* value objects: `GovernanceCycleId`, `CyclePeriod`, `CycleItem`, `CycleStatus`
* invariants: cycle cannot close with unresolved items unless explicit exception recorded; closed cycles are immutable

Completion proof: cycle lifecycle; item tracking; closure gate invariant fires correctly

---

**2.7B.11 Governance Record Domain — Full D2**

Purpose: produce the immutable governance evidence record after a decision process completes

Implementation topics:

* `GovernanceRecordAggregate` — full business logic
* lifecycle: `Pending → Finalized → Archived`
* reference linkage (decision, approval, quorum, guardian records)
* evidence completeness check
* tamper-evidence hashing
* governance record events: `GovernanceRecordCreatedEvent`, `GovernanceRecordFinalizedEvent`, `GovernanceRecordArchivedEvent`
* value objects: `GovernanceRecordId`, `GovernanceRecordReference`, `EvidencePack`, `RecordHash`
* invariants: record must reference at minimum one decision and one approval before finalization; finalized records are immutable; hash must be computed from all linked evidence deterministically

Completion proof: record finalization; evidence completeness gate; hash determinism on replay

---

**2.7B.12 Dispute Domain — Full D2**

Purpose: manage challenges to governance decisions or process outcomes

Implementation topics:

* `DisputeAggregate` — full business logic
* lifecycle: `Filed → UnderReview → Resolved / Dismissed → Closed`
* dispute classification (process challenge, authority challenge, outcome challenge)
* dispute resolution record
* escalation path
* dispute events: `DisputeFiledEvent`, `DisputeReviewStartedEvent`, `DisputeResolvedEvent`, `DisputeDismissedEvent`, `DisputeEscalatedEvent`, `DisputeClosedEvent`
* value objects: `DisputeId`, `DisputeClassification`, `DisputeResolution`, `EscalationRef`
* invariants: dispute must reference a valid governance record; resolved disputes cannot be re-filed; escalation path must be policy-authorized

Completion proof: dispute lifecycle; resolution recording; escalation path

---

**2.7B.13 Appeal Domain — Full D2**

Purpose: provide a structured path for contesting denied or adverse governance decisions

Implementation topics:

* `AppealAggregate` — full business logic
* lifecycle: `Filed → Admitted / Rejected → UnderReview → Upheld / Dismissed → Closed`
* appeal window enforcement (appeal must be filed within defined period)
* appeal grounds declaration
* review assignment
* appeal events: `AppealFiledEvent`, `AppealAdmittedEvent`, `AppealRejectedEvent`, `AppealUpheldEvent`, `AppealDismissedEvent`, `AppealClosedEvent`
* value objects: `AppealId`, `AppealGrounds`, `AppealWindow`, `AppealOutcome`
* invariants: appeal must reference original decision; appeals outside the appeal window are rejected; upheld appeals trigger re-review of original decision

Completion proof: appeal lifecycle; window enforcement; uphold path triggers re-review signal

---

**2.7B.14 Access Review Domain — Full D2**

Purpose: track periodic reviews of actor access within governance scope

Implementation topics:

* `AccessReviewAggregate` — full business logic
* lifecycle: `Scheduled → InProgress → Completed / Extended → Closed`
* review scope (actors, resources, access types)
* reviewer assignment
* review finding recording
* remediation linkage
* access review events: `AccessReviewScheduledEvent`, `AccessReviewStartedEvent`, `AccessReviewCompletedEvent`, `AccessReviewExtendedEvent`, `AccessReviewFindingRecordedEvent`, `AccessReviewClosedEvent`
* value objects: `AccessReviewId`, `ReviewScope`, `ReviewFinding`, `RemediationRef`, `ReviewDeadline`
* invariants: review must have defined scope before start; completed reviews are immutable; findings must reference specific access entries

Completion proof: review lifecycle; finding recording; completion gate correctness

---

**2.7B.15 Compliance Review Domain — Full D2**

Purpose: govern formal compliance checks triggered by governance bodies

Implementation topics:

* `ComplianceReviewAggregate` — full business logic
* lifecycle: `Initiated → InProgress → Completed / Escalated → Closed`
* compliance standard linkage
* review scope
* finding classification (conformant, non-conformant, exception)
* evidence collection linkage
* compliance review events: `ComplianceReviewInitiatedEvent`, `ComplianceReviewCompletedEvent`, `ComplianceReviewEscalatedEvent`, `ComplianceFindingRecordedEvent`, `ComplianceReviewClosedEvent`
* value objects: `ComplianceReviewId`, `ComplianceStandardRef`, `ComplianceFinding`, `EvidenceRef`, `EscalationRef`
* invariants: compliance review must reference a standard; non-conformant findings require evidence; escalated reviews must be policy-authorized

Completion proof: compliance review lifecycle; finding classification; escalation path

---

**2.7B.16 Exception Domain — Full D2**

Purpose: manage formally authorized exceptions to governance rules or policy constraints

Implementation topics:

* `ExceptionAggregate` — full business logic
* lifecycle: `Requested → Approved / Denied → Active / Expired → Closed`
* exception scope (which rule, which context, which period)
* approval chain requirement
* expiry enforcement
* exception events: `ExceptionRequestedEvent`, `ExceptionApprovedEvent`, `ExceptionDeniedEvent`, `ExceptionActivatedEvent`, `ExceptionExpiredEvent`, `ExceptionClosedEvent`
* value objects: `ExceptionId`, `ExceptionScope`, `ExceptionPeriod`, `ExceptionBasis`
* invariants: exception must reference the specific rule being excepted; exceptions without expiry are forbidden; expired exceptions cannot be reactivated without re-request

Completion proof: exception lifecycle; expiry enforcement; scope reference validation

---

**2.7B.17 Cluster Decision Domain — Full D2**

Purpose: record and govern decisions made at cluster level with quorum and guardian backing

Implementation topics:

* `ClusterDecisionAggregate` — full business logic
* lifecycle: `Initiated → Deliberating → Decided → Recorded → Archived`
* quorum reference linkage
* guardian reference linkage
* decision record production
* cluster decision events: `ClusterDecisionInitiatedEvent`, `ClusterDecisionDeclaredEvent`, `ClusterDecisionRecordedEvent`, `ClusterDecisionArchivedEvent`
* value objects: `ClusterDecisionId`, `DecisionOutcome`, `QuorumRef`, `GuardianRef`, `DecisionRecord`
* invariants: cluster decision must have both quorum met and guardian oversight recorded before being declared; decision without quorum ref is invalid; archived decision is immutable

Completion proof: cluster decision lifecycle; quorum and guardian linkage required; immutability after archive

---

**2.7B.18 Engine layer — governance T2E engine**

Purpose: implement command handlers for all governance context BCs

Implementation topics:

* `GuardianAppointCommandHandler` — load-then-guard idempotency (3-layer, per INV-IDEMPOTENT-LIFECYCLE-INIT-01)
* `GuardianRecordOversightCommandHandler`
* `GuardianDeclareConflictCommandHandler`
* `GuardianRetireCommandHandler`
* `QuorumDefineCommandHandler` — load-then-guard idempotency
* `QuorumStartVerificationCommandHandler`
* `QuorumRecordVoteCommandHandler`
* `QuorumEvaluateCommandHandler`
* `QuorumRecordCommandHandler`
* `ApprovalRequestCommandHandler`
* `ApprovalGrantCommandHandler`
* `ApprovalDenyCommandHandler`
* `ProposalSubmitCommandHandler`
* `ProposalAcceptCommandHandler`
* `ProposalRejectCommandHandler`
* `CommitteeDefineCommandHandler`
* `CommitteeAddMemberCommandHandler`
* `DelegationGrantCommandHandler`
* `DelegationRevokeCommandHandler`
* `MandateIssueCommandHandler`
* `MandateRecordEvidenceCommandHandler`
* `DisputeFileCommandHandler`
* `DisputeResolveCommandHandler`
* `AppealFileCommandHandler`
* `AppealReviewCommandHandler`
* `ClusterDecisionInitiateCommandHandler`
* `ClusterDecisionDeclareCommandHandler`
* `GovernanceRecordFinalizeCommandHandler`
* all handlers: policy-gated, deterministic, idempotent, event-sourced
* all handlers: raise typed domain errors on invariant violations per INV-502

Completion proof: all handlers produce correct events or typed errors; policy context present on every dispatch; idempotency passes under duplicate command

---

**2.7B.19 Policy integration layer**

Purpose: wire governance operations into WHYCEPOLICY

Implementation topics:

* guardian policy package — appointment, oversight, conflict, retirement
* quorum policy package — definition, verification, vote recording, evaluation
* approval policy package — request, grant, deny, escalation
* proposal policy package — submission, acceptance, rejection
* committee policy package — definition, membership, dissolution
* delegation policy package — grant, scope constraints, chain depth limits
* mandate policy package — issuance, fulfilment, revocation
* dispute policy package — filing, review, escalation
* appeal policy package — admission window, uphold/dismiss
* exception policy package — request, approval, expiry
* cluster decision policy package — initiation, quorum gate, guardian gate, declaration
* governance record policy package — finalization, archiving
* simulation coverage for all governance policy paths
* multi-party approval policy requirements where applicable
* emergency governance exception policy

Completion proof: every governance command resolves a policy; no governance mutation bypasses policy gate; simulation passes for all major paths

---

**2.7B.20 Runtime integration layer**

Purpose: wire governance domains through the 8-stage runtime pipeline

Implementation topics:

* governance command context propagation
* guardian scope enforcement at policy middleware
* quorum gate middleware integration (verify quorum met before binding governance actions)
* cluster decision composite gate (quorum + guardian both resolved)
* delegation scope propagation through identity context
* governance correlation ID continuity
* governance-specific trace context
* HSID stamping for all governance commands
* governance deny path canonical reason mapping
* governance escalation routing at runtime level

Completion proof: all governance commands flow through the full 8-stage pipeline; quorum and guardian gates fire correctly; deny path produces canonical reasons

---

**2.7B.21 Platform API integration layer**

Purpose: expose governance operations through canonical REST endpoints

Implementation topics:

* `POST /api/governance/guardians` — appoint guardian
* `POST /api/governance/guardians/{id}/oversight` — record oversight
* `POST /api/governance/guardians/{id}/conflict` — declare conflict
* `POST /api/governance/guardians/{id}/retire` — retire guardian
* `POST /api/governance/quorums` — define quorum
* `POST /api/governance/quorums/{id}/verification` — start verification
* `POST /api/governance/quorums/{id}/vote` — record vote
* `POST /api/governance/quorums/{id}/evaluate` — evaluate quorum
* `POST /api/governance/approvals` — request approval
* `POST /api/governance/approvals/{id}/grant` — grant approval
* `POST /api/governance/approvals/{id}/deny` — deny approval
* `POST /api/governance/proposals` — submit proposal
* `POST /api/governance/proposals/{id}/accept` — accept proposal
* `POST /api/governance/proposals/{id}/reject` — reject proposal
* `POST /api/governance/committees` — define committee
* `POST /api/governance/delegations` — grant delegation
* `POST /api/governance/mandates` — issue mandate
* `POST /api/governance/disputes` — file dispute
* `POST /api/governance/appeals` — file appeal
* `POST /api/governance/exceptions` — request exception
* `POST /api/governance/cluster-decisions` — initiate cluster decision
* `POST /api/governance/records/{id}/finalize` — finalize governance record
* `GET /api/governance/guardians/{id}` — read guardian
* `GET /api/governance/quorums/{id}` — read quorum
* `GET /api/governance/approvals/{id}` — read approval
* `GET /api/governance/records/{id}` — read governance record
* canonical route alignment per platform API conventions
* OpenAPI documentation for all governance endpoints

Completion proof: all endpoints respond correctly; policy enforcement visible in response headers; guardian/quorum gates enforce correctly at API layer

---

**2.7B.22 Persistence and event sourcing layer**

Implementation topics:

* event stream definitions for all governance BCs
* event versioning rules for guardian and quorum events
* deterministic event serialization (all value objects have JsonConverters)
* `GuardianAggregate` replay round-trip test
* `QuorumAggregate` replay round-trip test
* optimistic concurrency enforcement for all governance aggregates
* Postgres schema for governance event streams
* governance record evidence pack persistence linkage
* replay safety for quorum tally reconstruction
* recovery integrity for partial quorum verification

Completion proof: all governance aggregates replay correctly; no silent data loss in JSON round trip; optimistic concurrency fires on conflict

---

**2.7B.23 Messaging and Kafka layer**

Implementation topics:

* governance command topics
* governance event topics
* quorum verification event topics
* guardian oversight event topics
* cluster decision event topics
* governance record finalization topics
* retry topics for governance long-running operations
* deadletter topics with governance-specific classification
* topic naming compliance per canonical naming rules
* event contract registration for all governance events
* canonical header contract (HSID, correlation, causation, policy hash)
* outbox integration for governance event publication
* governance-specific retry behavior (quorum verification timeout handling)
* DLQ behavior for governance command failures

Completion proof: all governance events publish correctly; DLQ correctly captures failed governance commands; canonical headers present on all messages

---

**2.7B.24 Projection and read-model layer**

Implementation topics:

* `GuardianReadModel` — guardian status, scope, oversight history, conflict records
* `QuorumReadModel` — quorum status, tally, member participation, threshold, verification history
* `ApprovalReadModel` — approval chain status, approver states, outcome
* `ProposalReadModel` — proposal status, classification, review history
* `CommitteeReadModel` — committee status, membership, mandate linkage
* `DelegationReadModel` — delegation status, scope, chain depth, expiry
* `MandateReadModel` — mandate status, obligations, fulfilment records
* `DisputeReadModel` — dispute status, classification, resolution
* `AppealReadModel` — appeal status, grounds, outcome
* `ClusterDecisionReadModel` — decision status, quorum ref, guardian ref, outcome
* `GovernanceRecordReadModel` — finalized record with full evidence pack linkage
* governance summary projection — aggregate overview of active governance bodies and open decisions
* replay/catch-up validation for all governance projections

Completion proof: all governance read models populated correctly; catch-up from empty-state replay converges; quorum tally correctly reflected in read model

---

**2.7B.25 Cross-domain integration layer**

Implementation topics:

* `decision-system/governance` → `constitutional-system` (policy decision chain anchoring for governance decisions)
* `decision-system/governance` → `trust-system` (guardian identity resolution from WhyceID; trust score input for quorum weight)
* `decision-system/governance` → `economic-system` (governance decisions affecting economic actions feed through approval and quorum gates)
* `decision-system/governance` → `decision-system/audit` (governance record production → audit trail linkage)
* `decision-system/governance` → `decision-system/compliance` (compliance review referencing governance findings)
* `orchestration-system` → `governance` (workflow steps that require approval or quorum gate call governance commands)
* cross-domain causation chain integrity for governance-originated mutations
* no cross-BC direct domain references (only events and shared kernel)
* anti-drift checks on governance propagation

Completion proof: governance decision evidence links to chain anchors; trust input feeds quorum weight correctly; economic actions blocked without governance gate where required

---

**2.7B.26 Security and hardening layer**

Implementation topics:

* guardian scope cannot be self-assigned by the guardian's own identity
* quorum member cannot vote if flagged as conflict of interest
* delegation chain depth limit enforced at engine level
* exception scope cannot be self-approved
* appeal admission window enforced deterministically
* cluster decision cannot be declared by a single actor without quorum
* governance record finalization requires distinct identity from proposer
* anti-enumeration on governance record IDs
* privileged governance actions require elevated policy authorization (escalation path)

Completion proof: self-assignment attempt blocked; conflict-of-interest vote rejected; chain depth limit enforced; single-actor declaration rejected

---

**2.7B.27 Observability layer**

Implementation topics:

* guardian appointment and oversight metrics
* quorum verification time metrics (definition to evaluation)
* quorum met vs not-met rate metrics
* approval chain cycle time metrics
* proposal acceptance rate metrics
* delegation active scope count
* mandate fulfilment rate
* dispute and appeal volume signals
* exception active count and expiry proximity signals
* cluster decision time-to-record metrics
* governance record finalization completeness signals
* guardian conflict declaration rate (anomaly signal)
* governance pipeline latency histogram

Completion proof: all metrics emit on corresponding events; governance dashboard queryable from read models; anomaly signals fire correctly on conflict declarations

---

**2.7B.28 Testing and certification layer**

Implementation topics:

* unit tests: `GuardianAggregate` full lifecycle
* unit tests: `QuorumAggregate` full lifecycle including all threshold types
* unit tests: quorum evaluation — absolute, percentage, supermajority, unanimous
* unit tests: all governance BC aggregates
* invariant tests: guardian conflict blocks oversight; quorum post-record amendment rejected; delegation chain depth limit
* replay determinism tests: `GuardianAggregate` and `QuorumAggregate` replay-safe round-trip
* middleware integration tests: governance commands through full 8-stage pipeline
* policy integration tests: all governance commands resolve policy correctly
* cross-host idempotency tests: duplicate guardian appoint; duplicate quorum define
* API end-to-end tests: guardian appointment to cluster decision record full path
* quorum gateway test: cluster decision blocked when quorum not met
* guardian oversight test: cluster decision blocked when no guardian oversight recorded
* appeal admission window enforcement test
* delegation scope constraint test
* exception expiry enforcement test
* governance record finalization completeness gate test
* resilience tests: quorum verification timeout; partial vote recording recovery
* regression certification pack

Completion proof: all tests pass; idempotency holds under duplicate commands; quorum and guardian gates enforce in integration tests

---

**2.7B.29 Resilience validation layer**

Implementation topics:

* duplicate guardian appointment resistance
* duplicate quorum definition resistance
* quorum verification restart after crash mid-verification
* partial vote recording recovery (some votes recorded, restart, continuation correct)
* governance record finalization recovery after partial evidence collection
* cluster decision replay after quorum re-evaluation
* Kafka outage recovery for governance event publication
* Postgres outage recovery for governance aggregate persistence
* projection catch-up recovery after governance event reprocessing
* delegation expiry recovery after restart
* mandate expiry recovery after restart

Completion proof: all resilience paths tested; no data loss on restart; projection convergence after catch-up

---

**2.7B.30 Documentation and anti-drift layer**

Implementation topics:

* Guardian domain canonical README (replace stub)
* Quorum domain canonical README (replace stub)
* per-BC README for all governance domains
* governance context overview document
* quorum threshold type reference
* guardian scope and conflict model reference
* cluster decision composite gate documentation
* governance command/event catalog
* governance policy catalog
* governance API catalog
* guard updates for governance rules (domain.guard.md governance subsection)
* audit updates for governance BC coverage
* completion evidence pack

Completion proof: all READMEs accurate; guard and audit references updated; completion evidence produced

---

**2.7B.31 Phase 2.7B completion criteria**

* all governance context BCs at D2 (all mandatory artifact folders populated with business logic)
* `GuardianAggregate` fully implemented with lifecycle, events, invariants, and replay safety
* `QuorumAggregate` fully implemented with all threshold types, vote tallying, and replay safety
* all remaining governance BCs implemented at D2
* engine command handlers implemented for all governance commands
* policy packages wired for all governance commands
* runtime pipeline correctly enforces quorum gate and guardian oversight gate
* persistence and event sourcing correct with replay safety
* messaging and Kafka integration correct
* all governance projections/read models populated correctly
* platform API endpoints complete and policy-enforced
* cross-domain integration with constitutional-system, trust-system, economic-system correct
* all tests pass (unit, invariant, integration, API, resilience)
* observability metrics emitting correctly
* documentation complete and anti-drift checks passing
* completion evidence produced

---

**Best execution order:**

**Batch A — primary domains (Guardian + Quorum)**

* 2.7B.0 foundation
* 2.7B.1 Guardian domain D2
* 2.7B.2 Quorum domain D2

**Batch B — governance core BCs**

* 2.7B.3 Approval
* 2.7B.4 Proposal
* 2.7B.5 Committee
* 2.7B.6 Authority
* 2.7B.7 Charter

**Batch C — governance operational BCs**

* 2.7B.8 Delegation
* 2.7B.9 Mandate
* 2.7B.10 Governance Cycle
* 2.7B.11 Governance Record
* 2.7B.17 Cluster Decision

**Batch D — governance adjudication BCs**

* 2.7B.12 Dispute
* 2.7B.13 Appeal
* 2.7B.14 Access Review
* 2.7B.15 Compliance Review
* 2.7B.16 Exception

**Batch E — cross-cutting wiring**

* 2.7B.18 Engine layer
* 2.7B.19 Policy integration
* 2.7B.20 Runtime integration
* 2.7B.21 Platform API
* 2.7B.22 Persistence
* 2.7B.23 Messaging

**Batch F — proof**

* 2.7B.24 Projections
* 2.7B.25 Cross-domain integration
* 2.7B.26 Security hardening
* 2.7B.27 Observability
* 2.7B.28 Testing and certification
* 2.7B.29 Resilience validation
* 2.7B.30 Documentation and anti-drift

**Condensed implementation sequence:**

1. Guardian domain D2
2. Quorum domain D2
3. Approval domain D2
4. Proposal domain D2
5. Committee, Authority, Charter D2
6. Delegation, Mandate, Governance Cycle D2
7. Governance Record, Cluster Decision D2
8. Dispute, Appeal, Access Review, Compliance Review, Exception D2
9. Engine command handlers
10. Policy packages
11. Runtime + API + Persistence + Messaging
12. Projections
13. Cross-domain integration + security hardening
14. Observability + testing + resilience + documentation

A good canonical one-line statement for this phase is:

**Phase 2.7B implements the full `decision-system/governance` context to D2, anchored by Guardian and Quorum as the constitutional decision-integrity enforcers, then wires the complete governance domain set through engine, runtime, policy, persistence, messaging, projections, API, and cross-domain integration.**

The natural next step is **Phase 2.7C for Policy Release and Enforcement** to complete the T0U policy lifecycle loop.
