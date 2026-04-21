namespace Whycespace.Domain.ContentSystem.Invariant.Ownership;

public sealed class ContentMustHaveValidOwnerPolicy
{
    public ContentOwnershipDecision Decide(Guid contentId, Guid? ownerId)
    {
        if (contentId == Guid.Empty)
            return ContentOwnershipDecision.Deny(ContentOwnershipReason.MissingContent);

        if (!ownerId.HasValue || ownerId.Value == Guid.Empty)
            return ContentOwnershipDecision.Deny(ContentOwnershipReason.MissingOwner);

        return ContentOwnershipDecision.Allow();
    }
}
