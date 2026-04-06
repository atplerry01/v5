namespace Whycespace.Engines.T2E.Business.Marketplace.ParticipantMarket;

public class ParticipantMarketEngine
{
    private readonly ParticipantMarketPolicyAdapter _policy;

    public ParticipantMarketEngine(ParticipantMarketPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<ParticipantMarketResult> ExecuteAsync(ParticipantMarketCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new ParticipantMarketResult(true, "Executed");
    }
}
