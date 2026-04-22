using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Streaming.DeliveryGovernance.EntitlementHook;

/// Name token identifying the upstream entitlement-of-record system (e.g. "stripe", "rights-mgr").
public readonly record struct SourceSystemRef
{
    public string Value { get; }

    public SourceSystemRef(string value)
    {
        Guard.Against(string.IsNullOrWhiteSpace(value), "SourceSystemRef cannot be empty.");
        Value = value.Trim();
    }
}
