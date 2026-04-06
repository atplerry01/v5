using Whycespace.Domain.ConstitutionalSystem.Chain.Ledger;
using Whycespace.Shared.Contracts.Domain.Constitutional.Chain;

namespace Whycespace.Runtime.Engine.Domain.Constitutional;

/// <summary>
/// Runtime implementation of IChainHashService — bridges to domain HashService.
/// </summary>
public sealed class ChainHashService : IChainHashService
{
    public string SerializePayload(object payload) => HashService.SerializePayload(payload);
}
