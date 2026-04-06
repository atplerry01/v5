namespace Whycespace.Engines.T2E.Business.Scheduler.Recurrence;

public class RecurrenceEngine
{
    private readonly RecurrencePolicyAdapter _policy;

    public RecurrenceEngine(RecurrencePolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<RecurrenceResult> ExecuteAsync(RecurrenceCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new RecurrenceResult(true, "Executed");
    }
}
