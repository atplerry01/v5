namespace Whycespace.Domain.BusinessSystem.Portfolio.Mandate;

public sealed class MandateSpecification
{
    public bool IsSatisfiedBy(MandateId id, MandateName name)
    {
        return id.Value != Guid.Empty
            && !string.IsNullOrWhiteSpace(name.Value);
    }
}
