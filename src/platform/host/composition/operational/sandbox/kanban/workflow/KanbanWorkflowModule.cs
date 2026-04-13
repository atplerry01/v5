using Microsoft.Extensions.DependencyInjection;
using Whycespace.Engines.T1M.Domains.Operational.Sandbox.Kanban.Steps;
using Whycespace.Engines.T1M.Domains.Operational.Sandbox.Kanban.Workflows;
using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Platform.Host.Composition.Operational.Sandbox.Kanban.Workflow;

/// <summary>
/// Kanban workflow module — T1M workflow step DI registrations and workflow registry bindings.
/// </summary>
public static class KanbanWorkflowModule
{
    public static IServiceCollection AddKanbanWorkflow(this IServiceCollection services)
    {
        services.AddTransient<ValidateCardStep>();
        services.AddTransient<MoveToReviewStep>();
        services.AddTransient<ApproveCardStep>();
        services.AddTransient<CompleteCardStep>();
        return services;
    }

    public static void RegisterWorkflows(IWorkflowRegistry workflow)
    {
        workflow.Register(CardApprovalWorkflowNames.Approve, new[]
        {
            typeof(ValidateCardStep),
            typeof(MoveToReviewStep),
            typeof(ApproveCardStep),
            typeof(CompleteCardStep)
        });
    }
}
