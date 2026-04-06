namespace Whycespace.Engines.T2E.Constitutional.Chain.Ledger;

public class ChainLedgerEngine
{
    private readonly ChainLedgerPolicyAdapter _policy;

    public ChainLedgerEngine(ChainLedgerPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<ChainLedgerResult> ExecuteAsync(ChainLedgerCommand command)
    {
        await _policy.EnforceAsync(command);

        // Domain logic — emit events only (no persistence)

        return new ChainLedgerResult(true, "Executed");
    }
}
