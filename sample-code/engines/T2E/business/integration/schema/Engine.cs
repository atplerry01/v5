namespace Whycespace.Engines.T2E.Business.Integration.Schema;

public class SchemaEngine
{
    private readonly SchemaPolicyAdapter _policy;

    public SchemaEngine(SchemaPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<SchemaResult> ExecuteAsync(SchemaCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new SchemaResult(true, "Executed");
    }
}
