using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Trust.Identity.Verification;

public sealed record InitiateVerificationCommand(
    Guid VerificationId,
    Guid IdentityReference,
    string ClaimType,
    DateTimeOffset InitiatedAt) : IHasAggregateId
{
    public Guid AggregateId => VerificationId;
}

public sealed record PassVerificationCommand(
    Guid VerificationId) : IHasAggregateId
{
    public Guid AggregateId => VerificationId;
}

public sealed record FailVerificationCommand(
    Guid VerificationId) : IHasAggregateId
{
    public Guid AggregateId => VerificationId;
}
