using Whycespace.Systems.Midstream.Wss.Router;

namespace Whycespace.Systems.Midstream.Heos.Orchestration;

/// <summary>
/// HEOS orchestrator — routes workforce requests to WSS workflow router.
/// MUST NOT instantiate workflows or execute steps.
/// MUST NOT call engines directly.
/// </summary>
public sealed class HeosOrchestrator
{
    private readonly WorkflowRouter _workflowRouter;

    public HeosOrchestrator(WorkflowRouter workflowRouter)
    {
        _workflowRouter = workflowRouter;
    }

    public WorkflowRouteResult HandleAsync(
        string workflowType,
        IReadOnlyDictionary<string, string> parameters)
    {
        var (workflowId, cluster, subcluster, domain, context) = workflowType switch
        {
            "heos.assign-participant" => ("workforce.assign-participant", "workforce", "assignment", "operational", "heos"),
            "heos.assign-task" => ("workforce.assign-task", "workforce", "assignment", "operational", "heos"),
            "heos.trigger-incentive" => ("workforce.trigger-incentive", "workforce", "incentive", "operational", "heos"),
            "heos.evaluate-performance" => ("workforce.evaluate-performance", "workforce", "performance", "operational", "heos"),
            _ => throw new InvalidOperationException($"Unknown HEOS workflow: {workflowType}")
        };

        return _workflowRouter.Route(new WorkflowRouteRequest(
            workflowId, cluster, subcluster, domain, context));
    }
}
