using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.OperationalSystem.Deployment.Activation;

public sealed record RegionActivationCreatedEvent(Guid ActivationId, string RegionId, string JurisdictionCode) : DomainEvent;
public sealed record RegionCanaryStartedEvent(Guid ActivationId, string RegionId, int TrafficPercent) : DomainEvent;
public sealed record RegionFullyActivatedEvent(Guid ActivationId, string RegionId) : DomainEvent;
public sealed record RegionHaltedEvent(Guid ActivationId, string RegionId, string Reason) : DomainEvent;
public sealed record RegionResumedEvent(Guid ActivationId, string RegionId) : DomainEvent;
public sealed record RegionDecommissionedEvent(Guid ActivationId, string RegionId) : DomainEvent;
public sealed record ExposureLevelChangedEvent(Guid ActivationId, string RegionId, string NewLevel, int TrafficPercent) : DomainEvent;
