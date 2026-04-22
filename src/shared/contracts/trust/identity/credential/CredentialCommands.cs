using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Trust.Identity.Credential;

public sealed record IssueCredentialCommand(
    Guid CredentialId,
    Guid IdentityReference,
    string CredentialType,
    DateTimeOffset IssuedAt,
    string? CredentialHash = null) : IHasAggregateId
{
    public Guid AggregateId => CredentialId;
}

public sealed record ActivateCredentialCommand(
    Guid CredentialId) : IHasAggregateId
{
    public Guid AggregateId => CredentialId;
}

public sealed record RevokeCredentialCommand(
    Guid CredentialId) : IHasAggregateId
{
    public Guid AggregateId => CredentialId;
}
