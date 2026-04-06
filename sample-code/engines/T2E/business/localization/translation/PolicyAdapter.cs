using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Localization.Translation;

public sealed class TranslationPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
