using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Business.Agreement.PartyGovernance.Signature;

public sealed record CreateSignatureCommand(Guid SignatureId) : IHasAggregateId
{
    public Guid AggregateId => SignatureId;
}

public sealed record SignSignatureCommand(Guid SignatureId) : IHasAggregateId
{
    public Guid AggregateId => SignatureId;
}

public sealed record RevokeSignatureCommand(Guid SignatureId) : IHasAggregateId
{
    public Guid AggregateId => SignatureId;
}
