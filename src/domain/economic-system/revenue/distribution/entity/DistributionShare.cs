using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Revenue.Distribution;

public sealed class Allocation : Entity
{
    public Guid RecipientId { get; private set; }
    public Amount Amount { get; private set; }
    public decimal SharePercentage { get; private set; }

    private Allocation() { }

    internal static Allocation Create(Guid recipientId, Amount amount, decimal sharePercentage)
    {
        return new Allocation
        {
            RecipientId = recipientId,
            Amount = amount,
            SharePercentage = sharePercentage
        };
    }
}
