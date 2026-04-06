using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.OperationalSystem.Deployment.Activation;

/// <summary>
/// Controls region activation lifecycle for controlled rollout.
/// Lifecycle: Inactive → Canary → Active → (Halted ↔ Canary) → Decommissioned
/// </summary>
public sealed class RegionActivationAggregate : AggregateRoot
{
    public string RegionId { get; private set; } = string.Empty;
    public string JurisdictionCode { get; private set; } = string.Empty;
    public ActivationStatus Status { get; private set; } = ActivationStatus.Inactive;
    public ExposureLevel Exposure { get; private set; } = ExposureLevel.ReadOnly;
    public string? HaltReason { get; private set; }

    public static RegionActivationAggregate Create(Guid id, string regionId, string jurisdictionCode)
    {
        var agg = new RegionActivationAggregate
        {
            Id = id,
            RegionId = regionId,
            JurisdictionCode = jurisdictionCode
        };
        agg.RaiseDomainEvent(new RegionActivationCreatedEvent(id, regionId, jurisdictionCode));
        return agg;
    }

    public void StartCanary(ExposureLevel exposure)
    {
        EnsureValidTransition(Status, ActivationStatus.Canary, ActivationStatus.IsValidTransition);
        Status = ActivationStatus.Canary;
        Exposure = exposure;
        HaltReason = null;
        RaiseDomainEvent(new RegionCanaryStartedEvent(Id, RegionId, exposure.TrafficPercent));
    }

    public void FullyActivate()
    {
        EnsureValidTransition(Status, ActivationStatus.Active, ActivationStatus.IsValidTransition);
        Status = ActivationStatus.Active;
        Exposure = ExposureLevel.Full;
        RaiseDomainEvent(new RegionFullyActivatedEvent(Id, RegionId));
    }

    public void Halt(string reason)
    {
        EnsureValidTransition(Status, ActivationStatus.Halted, ActivationStatus.IsValidTransition);
        Status = ActivationStatus.Halted;
        Exposure = ExposureLevel.ReadOnly;
        HaltReason = reason;
        RaiseDomainEvent(new RegionHaltedEvent(Id, RegionId, reason));
    }

    public void Resume()
    {
        EnsureValidTransition(Status, ActivationStatus.Canary, ActivationStatus.IsValidTransition);
        Status = ActivationStatus.Canary;
        Exposure = ExposureLevel.Canary;
        HaltReason = null;
        RaiseDomainEvent(new RegionResumedEvent(Id, RegionId));
    }

    public void Decommission()
    {
        EnsureValidTransition(Status, ActivationStatus.Decommissioned, ActivationStatus.IsValidTransition);
        Status = ActivationStatus.Decommissioned;
        Exposure = ExposureLevel.ReadOnly;
        RaiseDomainEvent(new RegionDecommissionedEvent(Id, RegionId));
    }

    public void SetExposure(ExposureLevel level)
    {
        EnsureInvariant(Status.IsOperational, "MustBeOperational",
            $"Cannot change exposure while region is {Status.Value}.");
        Exposure = level;
        RaiseDomainEvent(new ExposureLevelChangedEvent(Id, RegionId, level.Value, level.TrafficPercent));
    }
}
