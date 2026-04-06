using Whycespace.Runtime.ControlPlane.Middleware;
using Whycespace.Runtime.Dispatcher;
using Whycespace.Runtime.Engine;
using Whycespace.Runtime.EventFabric;
using Whycespace.Runtime.GuardExecution.Runtime;
using Whycespace.Runtime.Observability;
using Whycespace.Runtime.Reliability;
using Whycespace.Runtime.Routing;
using Whycespace.Runtime.Sharding;
using Whycespace.Runtime.Simulation;
using Whycespace.Runtime.WhyceChain;
using Whycespace.Runtime.Workflow;
using Whycespace.Runtime.Workflow.State;
using Whycespace.Shared.Contracts.Infrastructure.Locking;
using Whycespace.Shared.Primitives.Id;
using Whycespace.Shared.Primitives.Time;

namespace Whycespace.Runtime.ControlPlane;

public sealed class RuntimeControlPlaneBuilder
{
    private readonly WorkflowResolver _workflowResolver = new();
    private readonly EngineResolver _engineResolver = new();
    private readonly RetryPolicyEngine _retryPolicy = new();
    private readonly TimeoutManager _timeoutManager = new();
    private readonly DeadLetterQueue _deadLetterQueue = new();
    private readonly TraceManager _traceManager = new();
    private readonly MetricsCollector _metricsCollector = new();
    private IEventPublisher? _eventPublisher;
    private IWorkflowStateStore? _workflowStateStore;
    private IClock _clock = SystemClock.Instance;
    private IIdGenerator _idGenerator = DefaultGuidGenerator.Instance;
    private DecisionRecordingMiddleware? _decisionRecordingMiddleware;

    // Mandatory middleware — must be supplied before Build().
    private ValidationMiddleware? _validationMiddleware;
    private IdempotencyMiddleware? _idempotencyMiddleware;
    private AuthorizationMiddleware? _authorizationMiddleware;
    private PolicyMiddleware? _policyMiddleware;
    private ExecutionGuardMiddleware? _executionGuardMiddleware;

    // Guard Execution Engine middleware (dual-phase)
    private GuardMiddleware? _prePolicyGuardMiddleware;
    private GuardMiddleware? _postPolicyGuardMiddleware;

    // --- Typed registration for mandatory middleware ---

    public RuntimeControlPlaneBuilder UseValidation(ValidationMiddleware middleware)
    {
        ArgumentNullException.ThrowIfNull(middleware);
        _validationMiddleware = middleware;
        return this;
    }

    public RuntimeControlPlaneBuilder UseIdempotency(IdempotencyMiddleware middleware)
    {
        ArgumentNullException.ThrowIfNull(middleware);
        _idempotencyMiddleware = middleware;
        return this;
    }

    public RuntimeControlPlaneBuilder UseAuthorization(AuthorizationMiddleware middleware)
    {
        ArgumentNullException.ThrowIfNull(middleware);
        _authorizationMiddleware = middleware;
        return this;
    }

    public RuntimeControlPlaneBuilder UsePolicy(PolicyMiddleware middleware)
    {
        ArgumentNullException.ThrowIfNull(middleware);
        _policyMiddleware = middleware;
        return this;
    }

    public RuntimeControlPlaneBuilder UseExecutionGuard(ExecutionGuardMiddleware middleware)
    {
        ArgumentNullException.ThrowIfNull(middleware);
        _executionGuardMiddleware = middleware;
        return this;
    }

    public RuntimeControlPlaneBuilder UsePrePolicyGuard(GuardMiddleware middleware)
    {
        ArgumentNullException.ThrowIfNull(middleware);
        _prePolicyGuardMiddleware = middleware;
        return this;
    }

    public RuntimeControlPlaneBuilder UsePostPolicyGuard(GuardMiddleware middleware)
    {
        ArgumentNullException.ThrowIfNull(middleware);
        _postPolicyGuardMiddleware = middleware;
        return this;
    }

    public RuntimeControlPlaneBuilder UseEventPublisher(IEventPublisher eventPublisher)
    {
        ArgumentNullException.ThrowIfNull(eventPublisher);
        _eventPublisher = eventPublisher;
        return this;
    }

    public RuntimeControlPlaneBuilder UseWorkflowStateStore(IWorkflowStateStore stateStore)
    {
        ArgumentNullException.ThrowIfNull(stateStore);
        _workflowStateStore = stateStore;
        return this;
    }

    public RuntimeControlPlaneBuilder UseClock(IClock clock)
    {
        ArgumentNullException.ThrowIfNull(clock);
        _clock = clock;
        return this;
    }

    public RuntimeControlPlaneBuilder UseDecisionRecording(IDecisionRecorder recorder)
    {
        ArgumentNullException.ThrowIfNull(recorder);
        _decisionRecordingMiddleware = new DecisionRecordingMiddleware(recorder);
        return this;
    }

    public WorkflowResolver Workflows => _workflowResolver;
    public EngineResolver Engines => _engineResolver;
    public RetryPolicyEngine RetryPolicy => _retryPolicy;
    public TimeoutManager Timeouts => _timeoutManager;
    public DeadLetterQueue DeadLetters => _deadLetterQueue;
    public TraceManager Tracing => _traceManager;
    public MetricsCollector Metrics => _metricsCollector;

    public RuntimeControlPlane Build(DomainRouteResolver? routeResolver = null)
    {
        ValidateMandatoryDependencies();

        var resolvedRouteResolver = routeResolver ?? new DomainRouteResolver();

        // ── Middleware order is LOCKED ──
        // This is the canonical execution order. It cannot be changed at runtime.
        //
        //   DecisionRecording (outermost — captures all decisions for WhyceChain, optional)
        //   → Tracing (captures full execution span)
        //   → Metrics (inside tracing so duration excludes trace bookkeeping)
        //   → Validation (reject malformed commands early)
        //   → Idempotency (short-circuit duplicates before auth/policy cost)
        //   → Authorization (identity check)
        //   → PrePolicyGuard (GEE structural/runtime/behavioral guards — before policy)
        //   → Policy (WHYCEPOLICY™ enforcement — mandatory, ONLY ONE)
        //   → PostPolicyGuard (GEE policy/binding/projection guards — after policy)
        //   → ExecutionGuard (final gate + marks context as runtime-originated)
        //   → Dispatcher (terminal)

        var buildPipeline = new MiddlewarePipeline();
        if (_decisionRecordingMiddleware is not null)
            buildPipeline.Add(_decisionRecordingMiddleware);
        buildPipeline.Add(new TracingMiddleware(_traceManager));
        buildPipeline.Add(new MetricsMiddleware(_metricsCollector));
        buildPipeline.Add(_validationMiddleware!);
        buildPipeline.Add(_idempotencyMiddleware!);
        buildPipeline.Add(_authorizationMiddleware!);
        buildPipeline.Add(_prePolicyGuardMiddleware!);
        buildPipeline.Add(_policyMiddleware!);
        buildPipeline.Add(_postPolicyGuardMiddleware!);
        buildPipeline.Add(_executionGuardMiddleware!);

        _engineResolver.Lock();
        resolvedRouteResolver.Lock();

        var invoker = new EngineInvoker(_engineResolver);
        var stateManager = _workflowStateStore is not null
            ? new WorkflowStateManager(_workflowStateStore)
            : new WorkflowStateManager();
        var executor = new WorkflowExecutor(invoker, stateManager, _eventPublisher!, _retryPolicy, _timeoutManager, _deadLetterQueue, resolvedRouteResolver, _idGenerator);
        var orchestrator = new WorkflowOrchestrator(_workflowResolver, executor, stateManager, _idGenerator);
        var dispatcher = new CommandDispatcher(orchestrator, resolvedRouteResolver);

        return new RuntimeControlPlane(buildPipeline, dispatcher, _clock, new PartitionKeyResolver());
    }

    public SimulationEngine BuildSimulation()
    {
        ValidateMandatoryDependencies();

        var inspectorPipeline = new MiddlewarePipeline();
        if (_decisionRecordingMiddleware is not null)
            inspectorPipeline.Add(_decisionRecordingMiddleware);
        inspectorPipeline.Add(new TracingMiddleware(_traceManager));
        inspectorPipeline.Add(new MetricsMiddleware(_metricsCollector));
        inspectorPipeline.Add(_validationMiddleware!);
        inspectorPipeline.Add(_idempotencyMiddleware!);
        inspectorPipeline.Add(_authorizationMiddleware!);
        inspectorPipeline.Add(_prePolicyGuardMiddleware!);
        inspectorPipeline.Add(_policyMiddleware!);
        inspectorPipeline.Add(_postPolicyGuardMiddleware!);
        inspectorPipeline.Add(_executionGuardMiddleware!);

        _engineResolver.Lock();

        return new SimulationEngine(inspectorPipeline, _workflowResolver);
    }

    private void ValidateMandatoryDependencies()
    {
        if (_eventPublisher is null)
            throw new RuntimeControlPlaneException(
                "Control plane requires an IEventPublisher. Call UseEventPublisher() before Build().",
                "MISSING_EVENT_PUBLISHER");

        if (_validationMiddleware is null)
            throw new RuntimeControlPlaneException(
                "ValidationMiddleware is REQUIRED. Call UseValidation() before Build().",
                "MISSING_VALIDATION_MIDDLEWARE");

        if (_idempotencyMiddleware is null)
            throw new RuntimeControlPlaneException(
                "IdempotencyMiddleware is REQUIRED. Call UseIdempotency() before Build().",
                "MISSING_IDEMPOTENCY_MIDDLEWARE");

        if (_authorizationMiddleware is null)
            throw new RuntimeControlPlaneException(
                "AuthorizationMiddleware is REQUIRED. Call UseAuthorization() before Build().",
                "MISSING_AUTHORIZATION_MIDDLEWARE");

        if (_policyMiddleware is null)
            throw new RuntimeControlPlaneException(
                "WHYCEPOLICY middleware is REQUIRED. Call UsePolicy() before Build().",
                "MISSING_POLICY_MIDDLEWARE");

        if (_executionGuardMiddleware is null)
            throw new RuntimeControlPlaneException(
                "ExecutionGuardMiddleware is REQUIRED. Call UseExecutionGuard() before Build().",
                "MISSING_EXECUTION_GUARD_MIDDLEWARE");

        if (_prePolicyGuardMiddleware is null)
            throw new RuntimeControlPlaneException(
                "PrePolicyGuardMiddleware is REQUIRED. Call UsePrePolicyGuard() before Build().",
                "MISSING_PRE_POLICY_GUARD_MIDDLEWARE");

        if (_postPolicyGuardMiddleware is null)
            throw new RuntimeControlPlaneException(
                "PostPolicyGuardMiddleware is REQUIRED. Call UsePostPolicyGuard() before Build().",
                "MISSING_POST_POLICY_GUARD_MIDDLEWARE");
    }
}
