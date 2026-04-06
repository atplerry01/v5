namespace Whycespace.Domain.TrustSystem.Access.Grant;

/// <summary>
/// Topic: whyce.core-access-trust.access.granted
/// Command: AccessGrantCommand
/// </summary>
public sealed record AccessGrantedEvent(
    Guid AccessId,
    Guid IdentityId,
    string Resource,
    string Permission) : DomainEvent;
