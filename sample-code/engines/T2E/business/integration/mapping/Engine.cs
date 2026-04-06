namespace Whycespace.Engines.T2E.Business.Integration.Mapping;

public class MappingEngine
{
    private readonly MappingPolicyAdapter _policy;

    public MappingEngine(MappingPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<MappingResult> ExecuteAsync(MappingCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new MappingResult(true, "Executed");
    }
}
