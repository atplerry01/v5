# Phase 2.8 — Trust-System Command/Event/Policy Catalog

Generated: 2026-04-22
Classification: trust-system

---

## Commands

### identity / registry (5)

| Command | Policy ID | Handler | Engine |
|---------|-----------|---------|--------|
| `InitiateRegistrationCommand` | `whyce.trust.identity.registry.initiate` | `InitiateRegistrationHandler` | T2E |
| `VerifyRegistrationCommand` | `whyce.trust.identity.registry.verify` | `VerifyRegistrationHandler` | T2E |
| `ActivateRegistrationCommand` | `whyce.trust.identity.registry.activate` | `ActivateRegistrationHandler` | T2E |
| `RejectRegistrationCommand` | `whyce.trust.identity.registry.reject` | `RejectRegistrationHandler` | T2E |
| `LockRegistrationCommand` | `whyce.trust.identity.registry.lock` | `LockRegistrationHandler` | T2E |

### identity / profile (3)

| Command | Policy ID | Handler | Engine |
|---------|-----------|---------|--------|
| `CreateProfileCommand` | `whyce.trust.identity.profile.create` | `CreateProfileHandler` | T2E |
| `ActivateProfileCommand` | `whyce.trust.identity.profile.activate` | `ActivateProfileHandler` | T2E |
| `DeactivateProfileCommand` | `whyce.trust.identity.profile.deactivate` | `DeactivateProfileHandler` | T2E |

### identity / consent (3)

| Command | Policy ID | Handler | Engine |
|---------|-----------|---------|--------|
| `GrantConsentCommand` | `whyce.trust.identity.consent.grant` | `GrantConsentHandler` | T2E |
| `RevokeConsentCommand` | `whyce.trust.identity.consent.revoke` | `RevokeConsentHandler` | T2E |
| `ExpireConsentCommand` | `whyce.trust.identity.consent.expire` | `ExpireConsentHandler` | T2E |

### access / session (3)

| Command | Policy ID | Handler | Engine |
|---------|-----------|---------|--------|
| `OpenSessionCommand` | `whyce.trust.access.session.open` | `OpenSessionHandler` | T2E |
| `ExpireSessionCommand` | `whyce.trust.access.session.expire` | `ExpireSessionHandler` | T2E |
| `TerminateSessionCommand` | `whyce.trust.access.session.terminate` | `TerminateSessionHandler` | T2E |

### identity / credential (3)

| Command | Policy ID | Handler | Engine |
|---------|-----------|---------|--------|
| `IssueCredentialCommand` | `whyce.trust.identity.credential.issue` | `IssueCredentialHandler` | T2E |
| `ActivateCredentialCommand` | `whyce.trust.identity.credential.activate` | `ActivateCredentialHandler` | T2E |
| `RevokeCredentialCommand` | `whyce.trust.identity.credential.revoke` | `RevokeCredentialHandler` | T2E |

### identity / verification (3)

| Command | Policy ID | Handler | Engine |
|---------|-----------|---------|--------|
| `InitiateVerificationCommand` | `whyce.trust.identity.verification.initiate` | `InitiateVerificationHandler` | T2E |
| `PassVerificationCommand` | `whyce.trust.identity.verification.pass` | `PassVerificationHandler` | T2E |
| `FailVerificationCommand` | `whyce.trust.identity.verification.fail` | `FailVerificationHandler` | T2E |

**Total: 20 commands across 6 BCs**

---

## Domain Events (published to Kafka)

### whyce.trust.identity.registry.events

| Event | Schema | Projection Handler | Cross-Domain |
|-------|--------|--------------------|--------------|
| `RegistrationInitiatedEvent` | `RegistrationInitiatedEventSchema` | `RegistryProjectionHandler` | — |
| `RegistrationVerifiedEvent` | `RegistrationVerifiedEventSchema` | `RegistryProjectionHandler` | — |
| `RegistrationActivatedEvent` | `RegistrationActivatedEventSchema` | `RegistryProjectionHandler` | `RegistrationActivatedCrossDomainHandler` |
| `RegistrationRejectedEvent` | `RegistrationRejectedEventSchema` | `RegistryProjectionHandler` | — |
| `RegistrationLockedEvent` | `RegistrationLockedEventSchema` | `RegistryProjectionHandler` | — |

### whyce.trust.identity.profile.events

| Event | Schema | Projection Handler |
|-------|--------|--------------------|
| `ProfileCreatedEvent` | `ProfileCreatedEventSchema` | `ProfileProjectionHandler` |
| `ProfileActivatedEvent` | `ProfileActivatedEventSchema` | `ProfileProjectionHandler` |
| `ProfileDeactivatedEvent` | `ProfileDeactivatedEventSchema` | `ProfileProjectionHandler` |

### whyce.trust.identity.consent.events

| Event | Schema | Projection Handler |
|-------|--------|--------------------|
| `ConsentGrantedEvent` | `ConsentGrantedEventSchema` | `ConsentProjectionHandler` |
| `ConsentRevokedEvent` | `ConsentRevokedEventSchema` | `ConsentProjectionHandler` |
| `ConsentExpiredEvent` | `ConsentExpiredEventSchema` | `ConsentProjectionHandler` |

### whyce.trust.access.session.events

| Event | Schema | Projection Handler |
|-------|--------|--------------------|
| `SessionOpenedEvent` | `SessionOpenedEventSchema` | `SessionProjectionHandler` |
| `SessionExpiredEvent` | `SessionExpiredEventSchema` | `SessionProjectionHandler` |
| `SessionTerminatedEvent` | `SessionTerminatedEventSchema` | `SessionProjectionHandler` |

### whyce.trust.identity.credential.events

| Event | Schema | Projection Handler |
|-------|--------|--------------------|
| `CredentialIssuedEvent` | `CredentialIssuedEventSchema` | `CredentialProjectionHandler` |
| `CredentialActivatedEvent` | `CredentialActivatedEventSchema` | `CredentialProjectionHandler` |
| `CredentialRevokedEvent` | `CredentialRevokedEventSchema` | `CredentialProjectionHandler` |

### whyce.trust.identity.verification.events

| Event | Schema | Projection Handler |
|-------|--------|--------------------|
| `VerificationInitiatedEvent` | `VerificationInitiatedEventSchema` | `VerificationProjectionHandler` |
| `VerificationPassedEvent` | `VerificationPassedEventSchema` | `VerificationProjectionHandler` |
| `VerificationFailedEvent` | `VerificationFailedEventSchema` | `VerificationProjectionHandler` |

---

## Read Models (projections)

| Read Model | Projection Store | Topic | Consumer Group |
|------------|-----------------|-------|----------------|
| `RegistryReadModel` | `projection_trust_identity_registry.registry_read_model` | `whyce.trust.identity.registry.events` | `whyce.projection.trust.identity.registry` |
| `ProfileReadModel` | `projection_trust_identity_profile.profile_read_model` | `whyce.trust.identity.profile.events` | `whyce.projection.trust.identity.profile` |
| `ConsentReadModel` | `projection_trust_identity_consent.consent_read_model` | `whyce.trust.identity.consent.events` | `whyce.projection.trust.identity.consent` |
| `SessionReadModel` | `projection_trust_access_session.session_read_model` | `whyce.trust.access.session.events` | `whyce.projection.trust.access.session` |
| `CredentialReadModel` | `projection_trust_identity_credential.credential_read_model` | `whyce.trust.identity.credential.events` | `whyce.projection.trust.identity.credential` |
| `VerificationReadModel` | `projection_trust_identity_verification.verification_read_model` | `whyce.trust.identity.verification.events` | `whyce.projection.trust.identity.verification` |

---

## Platform API Endpoints

| Method | Route | Command | Auth |
|--------|-------|---------|------|
| `POST` | `/api/trust/identity/registry/initiate` | `InitiateRegistrationCommand` | `[Authorize]` |
| `POST` | `/api/trust/identity/registry/verify` | `VerifyRegistrationCommand` | `[Authorize]` |
| `POST` | `/api/trust/identity/registry/activate` | `ActivateRegistrationCommand` | `[Authorize]` |
| `POST` | `/api/trust/identity/registry/reject` | `RejectRegistrationCommand` | `[Authorize]` |
| `POST` | `/api/trust/identity/registry/lock` | `LockRegistrationCommand` | `[Authorize]` |
| `GET` | `/api/trust/identity/registry/{registryId}` | — reads `RegistryReadModel` | `[Authorize]` |

---

## Cross-Domain Integration

| Trigger Event | Handler | Target BCs | Idempotency Key |
|---------------|---------|-----------|-----------------|
| `RegistrationActivatedEvent` | `RegistrationActivatedCrossDomainHandler` | `structural/humancapital/participant`, `economic/subject/subject` | `registration-activated-cross-domain:{eventId}` |

Worker: `RegistrationActivatedCrossDomainWorker`
Consumer group: `whyce.integration.registration-activated-cross-domain`
Retry escalation: `KafkaRetryEscalator` (full retry-tier wiring)

---

## Security Controls

| Control | Implementation | Location |
|---------|---------------|----------|
| Credential hash enforcement | `CredentialHashValue` (min 20 chars) | `src/domain/trust-system/identity/credential/value-object/` |
| Registration lockout | `RegistryAggregate.LockOut(reason)` | Domain — `RegistrationStatus.Locked` |
| Identity throttle | `IIdentityThrottlePolicy` / `InMemoryIdentityThrottlePolicy` | `src/runtime/security/` |
| Session fingerprint | `CommandContext.TokenFingerprint` (SHA256[:16] of Bearer) | `HttpCallerIdentityAccessor` |
| System identity scope | `SystemIdentityScope.Begin(...)` on workers | Background service wrappers |

---

## Observability

Meter: `Whycespace.Trust.Identity`

| Signal | Instrument | Dimensions |
|--------|-----------|------------|
| `whyce.trust.registration.initiated` | Counter | `registration_type` |
| `whyce.trust.registration.verified` | Counter | `registration_type` |
| `whyce.trust.registration.activated` | Counter | `registration_type` |
| `whyce.trust.registration.rejected` | Counter | `reason` |
| `whyce.trust.registration.locked` | Counter | `reason` |
| `whyce.trust.verification.initiated` | Counter | `verification_type` |
| `whyce.trust.verification.passed` | Counter | `verification_type` |
| `whyce.trust.verification.failed` | Counter | `verification_type` |
| `whyce.trust.consent.granted` | Counter | `consent_type` |
| `whyce.trust.consent.revoked` | Counter | `consent_type` |
| `whyce.trust.consent.expired` | Counter | `consent_type` |
| `whyce.trust.session.opened` | Counter | `session_type` |
| `whyce.trust.session.expired` | Counter | `session_type` |
| `whyce.trust.session.terminated` | Counter | `reason` |
| `whyce.trust.credential.issued` | Counter | `credential_type` |
| `whyce.trust.credential.revoked` | Counter | `reason` |
| `whyce.trust.policy.denied` | Counter | `policy_id` |
| `whyce.trust.throttle.violation` | Counter | `key` |
| `whyce.trust.token.fingerprint_mismatch` | Counter | — |
