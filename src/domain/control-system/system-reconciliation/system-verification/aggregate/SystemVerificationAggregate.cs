using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ControlSystem.SystemReconciliation.SystemVerification;

public sealed class SystemVerificationAggregate : AggregateRoot
{
    public SystemVerificationId Id { get; private set; }
    public string TargetSystem { get; private set; } = string.Empty;
    public VerificationStatus Status { get; private set; }
    public string? FailureReason { get; private set; }
    public DateTimeOffset InitiatedAt { get; private set; }

    private SystemVerificationAggregate() { }

    public static SystemVerificationAggregate Initiate(
        SystemVerificationId id,
        string targetSystem,
        DateTimeOffset initiatedAt)
    {
        Guard.Against(string.IsNullOrEmpty(targetSystem), SystemVerificationErrors.TargetSystemMustNotBeEmpty().Message);

        var aggregate = new SystemVerificationAggregate();
        aggregate.RaiseDomainEvent(new SystemVerificationInitiatedEvent(id, targetSystem, initiatedAt));
        return aggregate;
    }

    public void Pass(DateTimeOffset passedAt)
    {
        Guard.Against(
            Status != VerificationStatus.Initiated,
            SystemVerificationErrors.VerificationAlreadyTerminated(Status).Message);

        RaiseDomainEvent(new SystemVerificationPassedEvent(Id, passedAt));
    }

    public void Fail(string failureReason, DateTimeOffset failedAt)
    {
        Guard.Against(
            Status != VerificationStatus.Initiated,
            SystemVerificationErrors.VerificationAlreadyTerminated(Status).Message);
        Guard.Against(string.IsNullOrEmpty(failureReason), SystemVerificationErrors.FailureReasonMustNotBeEmpty().Message);

        RaiseDomainEvent(new SystemVerificationFailedEvent(Id, failureReason, failedAt));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case SystemVerificationInitiatedEvent e:
                Id = e.Id;
                TargetSystem = e.TargetSystem;
                Status = VerificationStatus.Initiated;
                InitiatedAt = e.InitiatedAt;
                break;
            case SystemVerificationPassedEvent:
                Status = VerificationStatus.Passed;
                break;
            case SystemVerificationFailedEvent e:
                Status = VerificationStatus.Failed;
                FailureReason = e.FailureReason;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        Guard.Against(Id.Value is null, "SystemVerification must have a non-empty Id.");
        Guard.Against(string.IsNullOrEmpty(TargetSystem), "SystemVerification must have a non-empty TargetSystem.");
    }
}
