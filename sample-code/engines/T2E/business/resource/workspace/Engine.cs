namespace Whycespace.Engines.T2E.Business.Resource.Workspace;

public class WorkspaceEngine
{
    private readonly WorkspacePolicyAdapter _policy;

    public WorkspaceEngine(WorkspacePolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<WorkspaceResult> ExecuteAsync(WorkspaceCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new WorkspaceResult(true, "Executed");
    }
}
