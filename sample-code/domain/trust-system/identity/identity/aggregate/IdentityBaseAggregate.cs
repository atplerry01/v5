using Whycespace.Domain.SharedKernel;
using Whycespace.Domain.SharedKernel.Regulatory.Jurisdiction;

namespace Whycespace.Domain.TrustSystem.Identity.Identity;

public class IdentityBaseAggregate : AggregateRoot
{
    public JurisdictionId? JurisdictionId { get; private set; }
}
