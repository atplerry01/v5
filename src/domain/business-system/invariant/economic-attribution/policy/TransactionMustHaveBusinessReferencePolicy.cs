namespace Whycespace.Domain.BusinessSystem.Invariant.EconomicAttribution;

public sealed class TransactionMustHaveBusinessReferencePolicy
{
    public EconomicAttributionDecision Decide(Guid transactionId, Guid? businessReferenceId)
    {
        if (transactionId == Guid.Empty)
            return EconomicAttributionDecision.Deny(EconomicAttributionReason.MissingTransaction);

        if (!businessReferenceId.HasValue || businessReferenceId.Value == Guid.Empty)
            return EconomicAttributionDecision.Deny(EconomicAttributionReason.MissingBusinessReference);

        return EconomicAttributionDecision.Allow();
    }
}
