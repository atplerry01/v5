namespace Whycespace.Engines.T2E.Business.Notification.Preference;

public class PreferenceEngine
{
    private readonly PreferencePolicyAdapter _policy;

    public PreferenceEngine(PreferencePolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<PreferenceResult> ExecuteAsync(PreferenceCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new PreferenceResult(true, "Executed");
    }
}
