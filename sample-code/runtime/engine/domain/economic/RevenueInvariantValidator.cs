using Whycespace.Domain.EconomicSystem.Ledger.Obligation;
using Whycespace.Domain.EconomicSystem.Revenue.Revenue;
using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Shared.Contracts.Domain.Economic;

namespace Whycespace.Runtime.Engine.Domain.Economic;

/// <summary>
/// Runtime implementation of IRevenueInvariantValidator — bridges to domain RevenueInvariantService.
/// </summary>
public sealed class RevenueInvariantValidator : IRevenueInvariantValidator
{
    private readonly RevenueInvariantService _domainService = new();

    public RevenueValidationResult Validate(object totalAmount, object recognizedAmount, object obligationStatus)
    {
        if (totalAmount is not Money domainTotal)
            return RevenueValidationResult.Fail("Invalid total amount type");

        if (recognizedAmount is not Money domainRecognized)
            return RevenueValidationResult.Fail("Invalid recognized amount type");

        if (obligationStatus is not ObligationStatus domainStatus)
            return RevenueValidationResult.Fail("Invalid obligation status type");

        var result = _domainService.Validate(domainTotal, domainRecognized, domainStatus);
        return result.IsValid
            ? RevenueValidationResult.Success()
            : RevenueValidationResult.Fail(result.Error!);
    }
}
