using Whycespace.Domain.SharedKernel.Primitive.Identity;

namespace Whycespace.Domain.StructuralSystem.HumanCapital.Participant;

/// <summary>
/// Links a participant to a verified identity in the identity domain.
/// Acts as the cross-domain bridge without leaking identity logic.
/// </summary>
public sealed record ParticipantIdentityLink
{
    public IdentityId IdentityId { get; }
    public DateTimeOffset LinkedAt { get; }

    public ParticipantIdentityLink(IdentityId identityId, DateTimeOffset linkedAt)
    {
        IdentityId = identityId ?? throw new ArgumentNullException(nameof(identityId));
        LinkedAt = linkedAt;
    }
}
