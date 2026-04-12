using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.OrchestrationSystem.Workflow.Instance;

public static class InstanceErrors
{
    public static DomainException MissingId() =>
        new("InstanceId cannot be empty.");

    public static DomainException MissingContext(string detail) =>
        new($"InstanceContext is invalid. {detail}");

    public static InvalidOperationException InvalidStateTransition(InstanceStatus status, string action) =>
        new($"Cannot {action} an instance in {status} status.");
}
