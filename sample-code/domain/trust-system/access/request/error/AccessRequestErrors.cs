namespace Whycespace.Domain.TrustSystem.Access.Request;

public static class AccessRequestErrors
{
    public static DomainException DuplicateRequest(Guid requesterId, string resource)
        => new("REQUEST.DUPLICATE", $"Requester '{requesterId}' already has a pending request for '{resource}'.");

    public static DomainException NotFound(Guid requestId)
        => new("REQUEST.NOT_FOUND", $"Access request '{requestId}' not found.");

    public static DomainException SelfApproval()
        => new("REQUEST.SELF_APPROVAL", "Requester cannot approve their own access request.");
}
