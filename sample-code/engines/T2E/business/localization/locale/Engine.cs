namespace Whycespace.Engines.T2E.Business.Localization.Locale;

public class LocaleEngine
{
    private readonly LocalePolicyAdapter _policy;

    public LocaleEngine(LocalePolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<LocaleResult> ExecuteAsync(LocaleCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new LocaleResult(true, "Executed");
    }
}
