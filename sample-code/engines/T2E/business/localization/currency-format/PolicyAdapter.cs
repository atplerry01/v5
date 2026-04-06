using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Localization.CurrencyFormat;

public sealed class CurrencyFormatPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
