using Whycespace.Domain.EconomicSystem.Ledger.Obligation;
using Whycespace.Shared.Contracts.Domain.Economic;

namespace Whycespace.Runtime.Engine.Domain.Economic;

/// <summary>
/// Runtime implementation of IObligationStatusFactory — bridges to domain ObligationStatus.
/// </summary>
public sealed class ObligationStatusFactory : IObligationStatusFactory
{
    public object Create(string status)
    {
        return new ObligationStatus(status);
    }
}
