namespace Whycespace.Engines.T2E.Business.Agreement.Contract;

public class ContractEngine
{
    private readonly ContractPolicyAdapter _policy;

    public ContractEngine(ContractPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<ContractResult> ExecuteAsync(ContractCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new ContractResult(true, "Executed");
    }
}
