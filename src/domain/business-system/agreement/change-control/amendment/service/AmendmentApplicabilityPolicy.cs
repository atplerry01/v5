using Whycespace.Domain.BusinessSystem.Agreement.Commitment.Contract;

namespace Whycespace.Domain.BusinessSystem.Agreement.ChangeControl.Amendment;

public sealed class AmendmentApplicabilityPolicy
{
    public AmendmentApplicabilityDecision Decide(AmendmentStatus amendmentStatus, ContractStatus targetContractStatus)
    {
        if (amendmentStatus != AmendmentStatus.Draft)
            return AmendmentApplicabilityDecision.Deny(AmendmentApplicabilityReason.AmendmentNotInDraft);

        if (targetContractStatus != ContractStatus.Active)
            return AmendmentApplicabilityDecision.Deny(AmendmentApplicabilityReason.TargetContractNotActive);

        return AmendmentApplicabilityDecision.Allow();
    }
}
