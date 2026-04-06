using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.HumanCapital.Workforce;

public sealed class WorkforceMemberAggregate : AggregateRoot
{
    public WorkforceId WorkforceId { get; private set; } = null!;
    public EmploymentModel EmploymentModel { get; private set; } = null!;
    public SkillProfile SkillProfile { get; private set; } = null!;
    public Capacity Capacity { get; private set; } = Capacity.Full;
    public Availability Availability { get; private set; } = Availability.Unavailable;

    public static WorkforceMemberAggregate Create(Guid workforceId, EmploymentModel employmentModel, SkillProfile skillProfile, Capacity capacity, Availability availability)
    {
        var member = new WorkforceMemberAggregate();
        member.Apply(new WorkforceMemberOnboardedEvent(workforceId, employmentModel.ModelType));
        member.WorkforceId = new WorkforceId(workforceId);
        member.EmploymentModel = employmentModel;
        member.SkillProfile = skillProfile;
        member.Capacity = capacity;
        member.Availability = availability;
        return member;
    }

    public void UpdateCapacity(Capacity newCapacity)
    {
        if (newCapacity.Value < 0)
            throw new DomainException(WorkforceErrors.CapacityExceeded, "Capacity cannot be negative.");

        Apply(new WorkforceCapacityUpdatedEvent(WorkforceId.Value, newCapacity.Value));
    }

    public void ChangeAvailability(Availability newAvailability)
    {
        Apply(new WorkforceAvailabilityChangedEvent(WorkforceId.Value, newAvailability.Value));
    }

    private void Apply(WorkforceMemberOnboardedEvent e)
    {
        Id = e.WorkforceId;
        RaiseDomainEvent(e);
    }

    private void Apply(WorkforceCapacityUpdatedEvent e)
    {
        Capacity = new Capacity(e.NewCapacity);
        RaiseDomainEvent(e);
    }

    private void Apply(WorkforceAvailabilityChangedEvent e)
    {
        Availability = new Availability(e.NewAvailability);
        RaiseDomainEvent(e);
    }
}
