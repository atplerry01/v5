using Whycespace.Shared.Primitives.Id;

namespace Whycespace.Domain.TrustSystem.Identity.ServiceIdentity;

public sealed class ServiceIdentityAggregate : AggregateRoot
{
    public ServiceIdentityId ServiceIdentityId { get; private set; } = null!;
    public string ServiceName { get; private set; } = string.Empty;
    public ServiceType ServiceType { get; private set; } = null!;
    public ServiceIdentityStatus Status { get; private set; } = null!;
    public DateTimeOffset RegisteredAt { get; private set; }

    private readonly List<ServiceCredential> _credentials = [];
    public IReadOnlyList<ServiceCredential> Credentials => _credentials.AsReadOnly();

    private readonly List<ServiceScope> _scopes = [];
    public IReadOnlyList<ServiceScope> Scopes => _scopes.AsReadOnly();

    private ServiceIdentityAggregate() { }

    public static ServiceIdentityAggregate Register(string serviceName, ServiceType serviceType, DateTimeOffset timestamp)
    {
        Guard.AgainstEmpty(serviceName);
        Guard.AgainstNull(serviceType);

        var identity = new ServiceIdentityAggregate
        {
            ServiceIdentityId = ServiceIdentityId.FromSeed($"ServiceIdentity:{serviceName}:{serviceType.Value}"),
            ServiceName = serviceName,
            ServiceType = serviceType,
            Status = ServiceIdentityStatus.Active,
            RegisteredAt = timestamp
        };

        identity.Id = identity.ServiceIdentityId.Value;
        identity.RaiseDomainEvent(new ServiceIdentityRegisteredEvent(
            identity.ServiceIdentityId.Value, serviceName, serviceType.Value));
        return identity;
    }

    public ServiceCredential IssueCredential(DateTimeOffset expiresAt, DateTimeOffset timestamp)
    {
        EnsureInvariant(
            Status == ServiceIdentityStatus.Active,
            "SERVICE_MUST_BE_ACTIVE",
            "Cannot issue credentials for an inactive service.");

        var credential = ServiceCredential.Issue(ServiceIdentityId.Value, expiresAt, timestamp);
        _credentials.Add(credential);

        RaiseDomainEvent(new ServiceCredentialIssuedEvent(
            ServiceIdentityId.Value, credential.Id));
        return credential;
    }

    public void RevokeCredential(Guid credentialId, DateTimeOffset timestamp)
    {
        var credential = _credentials.FirstOrDefault(c => c.Id == credentialId && c.IsActive);
        EnsureInvariant(
            credential is not null,
            "CREDENTIAL_NOT_FOUND",
            $"Active credential '{credentialId}' not found.");

        credential!.Revoke(timestamp);
        RaiseDomainEvent(new ServiceCredentialRevokedEvent(
            ServiceIdentityId.Value, credentialId));
    }

    public void AddScope(ServiceScope scope)
    {
        Guard.AgainstNull(scope);
        EnsureInvariant(
            !_scopes.Any(s => s.Value == scope.Value),
            "SCOPE_ALREADY_EXISTS",
            $"Scope '{scope.Value}' already exists.");

        _scopes.Add(scope);
    }

    public void Suspend(string reason)
    {
        Guard.AgainstEmpty(reason);
        EnsureInvariant(
            Status == ServiceIdentityStatus.Active,
            "SERVICE_MUST_BE_ACTIVE",
            "Service is not active.");

        Status = ServiceIdentityStatus.Suspended;
        RaiseDomainEvent(new ServiceIdentitySuspendedEvent(
            ServiceIdentityId.Value, reason));
    }

    public void Reactivate()
    {
        EnsureInvariant(
            Status == ServiceIdentityStatus.Suspended,
            "SERVICE_MUST_BE_SUSPENDED",
            "Service is not suspended.");

        Status = ServiceIdentityStatus.Active;
        RaiseDomainEvent(new ServiceIdentityReactivatedEvent(ServiceIdentityId.Value));
    }

    public void Decommission(string reason)
    {
        Guard.AgainstEmpty(reason);
        EnsureNotTerminal(Status, s => s == ServiceIdentityStatus.Decommissioned, "Decommission");

        Status = ServiceIdentityStatus.Decommissioned;
        RaiseDomainEvent(new ServiceIdentityDecommissionedEvent(
            ServiceIdentityId.Value, reason));
    }
}
