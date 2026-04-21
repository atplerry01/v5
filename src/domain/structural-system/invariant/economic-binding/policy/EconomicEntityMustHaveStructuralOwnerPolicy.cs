namespace Whycespace.Domain.StructuralSystem.Invariant.EconomicBinding;

public sealed class EconomicEntityMustHaveStructuralOwnerPolicy
{
    public EconomicBindingDecision Decide(Guid economicEntityId, Guid? structuralOwnerId)
    {
        if (economicEntityId == Guid.Empty)
            return EconomicBindingDecision.Deny(EconomicBindingReason.MissingEconomicEntity);

        if (!structuralOwnerId.HasValue || structuralOwnerId.Value == Guid.Empty)
            return EconomicBindingDecision.Deny(EconomicBindingReason.MissingStructuralOwner);

        return EconomicBindingDecision.Allow();
    }
}
