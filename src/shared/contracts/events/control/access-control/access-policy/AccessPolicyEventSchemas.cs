namespace Whycespace.Shared.Contracts.Events.Control.AccessControl.AccessPolicy;

public sealed record AccessPolicyDefinedEventSchema(
    Guid AggregateId,
    string Name,
    string Scope,
    IReadOnlyList<string> AllowedRoleIds);

public sealed record AccessPolicyActivatedEventSchema(
    Guid AggregateId);

public sealed record AccessPolicyRetiredEventSchema(
    Guid AggregateId);
