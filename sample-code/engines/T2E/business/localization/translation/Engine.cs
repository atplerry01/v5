namespace Whycespace.Engines.T2E.Business.Localization.Translation;

public class TranslationEngine
{
    private readonly TranslationPolicyAdapter _policy;

    public TranslationEngine(TranslationPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<TranslationResult> ExecuteAsync(TranslationCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new TranslationResult(true, "Executed");
    }
}
