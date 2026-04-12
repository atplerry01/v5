namespace Whycespace.Domain.BusinessSystem.Entitlement.EntitlementGrant;

public sealed class EntitlementAssignment
{
    public GrantSubjectId SubjectId { get; }
    public EntitlementRightId RightId { get; }

    public EntitlementAssignment(GrantSubjectId subjectId, EntitlementRightId rightId)
    {
        if (subjectId == default)
            throw new ArgumentException("SubjectId must not be empty.", nameof(subjectId));

        if (rightId == default)
            throw new ArgumentException("RightId must not be empty.", nameof(rightId));

        SubjectId = subjectId;
        RightId = rightId;
    }
}
