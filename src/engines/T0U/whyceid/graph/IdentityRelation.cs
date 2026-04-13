namespace Whycespace.Engines.T0U.WhyceId.Graph;

/// <summary>
/// Represents a relationship between two identities.
/// </summary>
public sealed record IdentityRelation(
    string FromIdentityId,
    string ToIdentityId,
    string RelationType,
    string RelationHash);
