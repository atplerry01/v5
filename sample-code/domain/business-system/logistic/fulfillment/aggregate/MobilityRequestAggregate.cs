using Whycespace.Domain.SharedKernel.Primitive.Identity;

namespace Whycespace.Domain.BusinessSystem.Logistic.Fulfillment;

public sealed class MobilityRequestAggregate
{
    public Guid Id { get; }
    public IdentityId RequestorIdentityId { get; private set; } = default!;
}
