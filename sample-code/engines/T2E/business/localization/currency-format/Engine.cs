namespace Whycespace.Engines.T2E.Business.Localization.CurrencyFormat;

public class CurrencyFormatEngine
{
    private readonly CurrencyFormatPolicyAdapter _policy;

    public CurrencyFormatEngine(CurrencyFormatPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<CurrencyFormatResult> ExecuteAsync(CurrencyFormatCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new CurrencyFormatResult(true, "Executed");
    }
}
