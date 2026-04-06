namespace Whycespace.Engines.T2E.Business.Localization.Timezone;

public class TimezoneEngine
{
    private readonly TimezonePolicyAdapter _policy;

    public TimezoneEngine(TimezonePolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<TimezoneResult> ExecuteAsync(TimezoneCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new TimezoneResult(true, "Executed");
    }
}
