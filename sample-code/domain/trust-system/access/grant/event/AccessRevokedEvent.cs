namespace Whycespace.Domain.TrustSystem.Access.Grant;

public sealed record AccessRevokedEvent(
    Guid AccessId,
    Guid IdentityId,
    string Resource,
    string Permission,
    string Reason) : DomainEvent;
