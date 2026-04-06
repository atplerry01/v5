namespace Whycespace.Engines.T2E.Business.Agreement.Counterparty;

public class CounterpartyEngine
{
    private readonly CounterpartyPolicyAdapter _policy;

    public CounterpartyEngine(CounterpartyPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<CounterpartyResult> ExecuteAsync(CounterpartyCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new CounterpartyResult(true, "Executed");
    }
}
