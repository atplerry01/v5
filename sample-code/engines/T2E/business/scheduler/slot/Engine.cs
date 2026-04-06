namespace Whycespace.Engines.T2E.Business.Scheduler.Slot;

public class SlotEngine
{
    private readonly SlotPolicyAdapter _policy;

    public SlotEngine(SlotPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<SlotResult> ExecuteAsync(SlotCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new SlotResult(true, "Executed");
    }
}
