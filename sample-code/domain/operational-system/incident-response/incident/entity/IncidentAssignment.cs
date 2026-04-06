using Whycespace.Domain.SharedKernel.Primitive.Identity;
using Whycespace.Shared.Primitives.Id;

namespace Whycespace.Domain.OperationalSystem.IncidentResponse.Incident;

public sealed class IncidentAssignment
{
    public Guid Id { get; private set; }
    public IdentityId AssigneeIdentityId { get; private set; } = default!;
    public int EscalationLevel { get; private set; }
    public DateTimeOffset AssignedAt { get; private set; }
    public DateTimeOffset? AcknowledgedAt { get; private set; }

    private IncidentAssignment() { }

    public static IncidentAssignment Create(IdentityId assigneeIdentityId, int escalationLevel, DateTimeOffset timestamp)
    {
        return new IncidentAssignment
        {
            Id = DeterministicIdHelper.FromSeed($"IncidentAssignment:{assigneeIdentityId.Value}:{escalationLevel}"),
            AssigneeIdentityId = assigneeIdentityId,
            EscalationLevel = escalationLevel,
            AssignedAt = timestamp
        };
    }

    public void Acknowledge(DateTimeOffset timestamp)
    {
        AcknowledgedAt = timestamp;
    }
}
