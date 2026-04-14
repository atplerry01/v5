namespace Whycespace.Domain.EconomicSystem.Subject.Subject;

/// <summary>
/// Economic-layer subject classification. Bridges structural-system actors
/// into the economic domain. Note: <see cref="CWG"/> is an alias of structural
/// <c>Subcluster</c> at the economic boundary — no structural rename occurs.
/// </summary>
public enum SubjectType
{
    /// <summary>Individual actor (maps from structural Participant).</summary>
    Participant,

    /// <summary>
    /// Cluster Working Group — alias of structural <c>Subcluster</c> at the
    /// economic boundary. The structural-system does not carry a CWG name.
    /// </summary>
    CWG,

    /// <summary>Special Purpose Vehicle (maps from structural SPV).</summary>
    SPV,

    /// <summary>Provider actor (maps from structural Provider).</summary>
    Provider,

    /// <summary>Cluster actor (maps from structural Cluster).</summary>
    Cluster
}
