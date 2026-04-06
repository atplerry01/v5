namespace Whycespace.Engines.T2E.Business.Resource.Equipment;

public class EquipmentEngine
{
    private readonly EquipmentPolicyAdapter _policy;

    public EquipmentEngine(EquipmentPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<EquipmentResult> ExecuteAsync(EquipmentCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new EquipmentResult(true, "Executed");
    }
}
