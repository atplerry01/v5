using System.Diagnostics.Metrics;
using System.Threading.RateLimiting;
using Whyce.Shared.Contracts.Infrastructure.Admission;
using Whyce.Shared.Contracts.Runtime;

namespace Whyce.Runtime.Dispatcher;

/// <summary>
/// phase1.5-S5.2.2 / KC-6 (WORKFLOW-ADMISSION-01): declared in-flight
/// admission gate for workflow execution. Closes K-R-05 by enforcing
/// per-workflow-name and per-tenant concurrency ceilings sized from
/// <see cref="WorkflowOptions"/>.
///
/// The gate is composed of two independent
/// <see cref="PartitionedRateLimiter{TResource}"/> instances:
///
///   - <c>_workflowLimiter</c> partitioned by workflow name with
///     <c>PerWorkflowConcurrency</c> permits per partition.
///   - <c>_tenantLimiter</c> partitioned by tenant id with
///     <c>PerTenantConcurrency</c> permits per partition.
///
/// Both must grant a permit for a workflow start to proceed. If
/// either fails, the gate disposes any acquired lease and throws
/// <see cref="WorkflowSaturatedException"/> — the canonical
/// RETRYABLE REFUSAL path that the API edge maps to HTTP 503 +
/// <c>Retry-After</c>.
///
/// Reuses BCL <c>System.Threading.RateLimiting</c> primitives —
/// no custom limiter, no Polly, no hand-rolled coordination. The
/// pattern mirrors the §5.2.1 PC-1 intake limiter shape.
/// </summary>
public sealed class WorkflowAdmissionGate : IDisposable
{
    // phase1.5-S5.2.2 / KC-6: dedicated meter for the workflow
    // admission seam. Two counters tagged by workflow_name and
    // (for rejections) partition. Cardinality is bounded by the
    // declared workflow registry.
    public static readonly Meter Meter = new("Whyce.Workflow", "1.0");
    private static readonly Counter<long> AdmittedCounter =
        Meter.CreateCounter<long>("workflow.admitted");
    private static readonly Counter<long> RejectedCounter =
        Meter.CreateCounter<long>("workflow.rejected");

    private readonly WorkflowOptions _options;
    private readonly PartitionedRateLimiter<string> _workflowLimiter;
    private readonly PartitionedRateLimiter<string> _tenantLimiter;

    public WorkflowAdmissionGate(WorkflowOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (options.PerWorkflowConcurrency < 1)
            throw new ArgumentOutOfRangeException(
                nameof(options), options.PerWorkflowConcurrency,
                "WorkflowOptions.PerWorkflowConcurrency must be at least 1.");
        if (options.PerTenantConcurrency < 1)
            throw new ArgumentOutOfRangeException(
                nameof(options), options.PerTenantConcurrency,
                "WorkflowOptions.PerTenantConcurrency must be at least 1.");
        if (options.QueueLimit < 0)
            throw new ArgumentOutOfRangeException(
                nameof(options), options.QueueLimit,
                "WorkflowOptions.QueueLimit must be 0 or greater.");

        _options = options;

        _workflowLimiter = PartitionedRateLimiter.Create<string, string>(workflowName =>
            RateLimitPartition.GetConcurrencyLimiter(
                partitionKey: workflowName,
                factory: _ => new ConcurrencyLimiterOptions
                {
                    PermitLimit = options.PerWorkflowConcurrency,
                    QueueLimit = options.QueueLimit,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                }));

        _tenantLimiter = PartitionedRateLimiter.Create<string, string>(tenantId =>
            RateLimitPartition.GetConcurrencyLimiter(
                partitionKey: tenantId,
                factory: _ => new ConcurrencyLimiterOptions
                {
                    PermitLimit = options.PerTenantConcurrency,
                    QueueLimit = options.QueueLimit,
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                }));
    }

    /// <summary>
    /// Acquires a per-workflow-name lease and a per-tenant lease.
    /// Both must succeed for the call to return; either failure
    /// throws <see cref="WorkflowSaturatedException"/> after
    /// disposing any acquired lease. The returned
    /// <see cref="WorkflowAdmissionLease"/> must be disposed by the
    /// caller (typically via <c>await using</c>) to release both
    /// permits when the workflow execution completes.
    /// </summary>
    public async ValueTask<WorkflowAdmissionLease> AcquireAsync(
        string workflowName,
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        var workflowLease = await _workflowLimiter.AcquireAsync(workflowName, 1, cancellationToken);
        if (!workflowLease.IsAcquired)
        {
            workflowLease.Dispose();
            RejectedCounter.Add(1,
                new KeyValuePair<string, object?>("workflow_name", workflowName),
                new KeyValuePair<string, object?>("partition", "workflow"));
            throw new WorkflowSaturatedException(workflowName, "workflow", _options.RetryAfterSeconds);
        }

        var tenantLease = await _tenantLimiter.AcquireAsync(tenantId, 1, cancellationToken);
        if (!tenantLease.IsAcquired)
        {
            tenantLease.Dispose();
            workflowLease.Dispose();
            RejectedCounter.Add(1,
                new KeyValuePair<string, object?>("workflow_name", workflowName),
                new KeyValuePair<string, object?>("partition", "tenant"));
            throw new WorkflowSaturatedException(workflowName, "tenant", _options.RetryAfterSeconds);
        }

        AdmittedCounter.Add(1,
            new KeyValuePair<string, object?>("workflow_name", workflowName));
        return new WorkflowAdmissionLease(workflowLease, tenantLease);
    }

    public void Dispose()
    {
        _workflowLimiter.Dispose();
        _tenantLimiter.Dispose();
    }
}

/// <summary>
/// Composite lease wrapping the per-workflow-name and per-tenant
/// limiter leases. Disposing releases both permits in reverse
/// acquisition order (tenant then workflow).
/// </summary>
public readonly struct WorkflowAdmissionLease : IDisposable
{
    private readonly RateLimitLease _workflowLease;
    private readonly RateLimitLease _tenantLease;

    internal WorkflowAdmissionLease(RateLimitLease workflowLease, RateLimitLease tenantLease)
    {
        _workflowLease = workflowLease;
        _tenantLease = tenantLease;
    }

    public void Dispose()
    {
        _tenantLease.Dispose();
        _workflowLease.Dispose();
    }
}
