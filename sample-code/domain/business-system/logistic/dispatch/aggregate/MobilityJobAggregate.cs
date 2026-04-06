using Whycespace.Domain.SharedKernel;
using Whycespace.Domain.BusinessSystem.Logistic;

namespace Whycespace.Domain.BusinessSystem.Logistic.Dispatch;

public class MobilityJobAggregate : AggregateRoot
{
    protected MobilityJobAggregate() { }

    public JobId JobId { get; private set; } = default!;
    public Guid OperatorId { get; private set; }
    public JobStatus Status { get; private set; } = JobStatus.Requested;
    public Location StartLocation { get; private set; } = Location.Unknown;
    public Location EndLocation { get; private set; } = Location.Unknown;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    public static MobilityJobAggregate RequestJob(
        Guid operatorId,
        Location startLocation,
        Location endLocation)
    {
        Guard.AgainstDefault(operatorId);
        Guard.AgainstNull(startLocation);
        Guard.AgainstNull(endLocation);
        Guard.AgainstInvalid(startLocation, l => l != Location.Unknown, "Start location is required.");
        Guard.AgainstInvalid(endLocation, l => l != Location.Unknown, "End location is required.");

        var job = new MobilityJobAggregate();
        var jobId = JobId.FromSeed($"MobilityJob:{operatorId}:{startLocation.Latitude}:{startLocation.Longitude}");

        job.Apply(new JobRequestedEvent(
            jobId.Value,
            operatorId,
            startLocation.Latitude,
            startLocation.Longitude,
            startLocation.Label,
            endLocation.Latitude,
            endLocation.Longitude,
            endLocation.Label));

        return job;
    }

    public void AssignOperator(Guid operatorId)
    {
        Guard.AgainstDefault(operatorId);

        if (Status.IsTerminal)
            throw new DomainException(DispatchErrors.InvalidTransition,
                $"Cannot assign operator to a job in '{Status.Value}' status.");

        if (Status != JobStatus.Requested)
            throw new DomainException(DispatchErrors.InvalidTransition,
                $"Job must be in 'requested' status to assign an operator.");

        Apply(new JobAssignedEvent(Id, operatorId));
    }

    public void StartJob()
    {
        if (Status != JobStatus.Assigned)
            throw new DomainException(DispatchErrors.NotAssigned,
                "Job must be assigned before it can be started.");

        Apply(new JobStartedEvent(Id));
    }

    public void CompleteJob()
    {
        if (Status == JobStatus.Completed)
            throw new DomainException(DispatchErrors.AlreadyCompleted,
                "Job is already completed.");

        if (Status != JobStatus.InProgress)
            throw new DomainException(DispatchErrors.NotInProgress,
                "Job must be in progress to be completed.");

        Apply(new JobCompletedEvent(Id));
    }

    public void CancelJob(string reason)
    {
        Guard.AgainstEmpty(reason);

        if (Status == JobStatus.Cancelled)
            throw new DomainException(DispatchErrors.AlreadyCancelled,
                "Job is already cancelled.");

        if (Status.IsTerminal)
            throw new DomainException(DispatchErrors.InvalidTransition,
                $"Cannot cancel a job in '{Status.Value}' status.");

        Apply(new JobCancelledEvent(Id, reason));
    }

    private void Apply(JobRequestedEvent e)
    {
        Id = e.JobId;
        JobId = new JobId(e.JobId);
        OperatorId = e.OperatorId;
        Status = JobStatus.Requested;
        StartLocation = new Location(e.StartLatitude, e.StartLongitude, e.StartLabel);
        EndLocation = new Location(e.EndLatitude, e.EndLongitude, e.EndLabel);
        CreatedAt = e.OccurredAt;
        RaiseDomainEvent(e);
    }

    private void Apply(JobAssignedEvent e)
    {
        OperatorId = e.OperatorId;
        Status = JobStatus.Assigned;
        RaiseDomainEvent(e);
    }

    private void Apply(JobStartedEvent e)
    {
        Status = JobStatus.InProgress;
        RaiseDomainEvent(e);
    }

    private void Apply(JobCompletedEvent e)
    {
        Status = JobStatus.Completed;
        CompletedAt = e.OccurredAt;
        RaiseDomainEvent(e);
    }

    private void Apply(JobCancelledEvent e)
    {
        Status = JobStatus.Cancelled;
        RaiseDomainEvent(e);
    }
}
