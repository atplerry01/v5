using Whycespace.Shared.Contracts.Policy;

namespace Whycespace.Engines.T2E.Business.Document.ContractDocument;

public sealed class ContractDocumentPolicyAdapter : PolicyAdapterBase
{
    protected override Task ApplyConditionsAsync(PolicyEvaluationResult result)
    {
        return Task.CompletedTask;
    }
}
