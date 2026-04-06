using System.Text;

namespace Whycespace.Engines.T0U.WhyceId;

/// <summary>
/// Maps identity aggregate events to canonical Kafka topic channels.
/// Follows the WBSM v3 canonical topic pattern:
///   whyce.identity.access.{domain}.events
///
/// All domain events for a given BC route to the single `.events` channel.
/// Used by the event fabric to route identity domain events to the correct
/// Kafka topics for projection consumers and cross-domain subscribers.
/// </summary>
public static class IdentityEventTopicMapper
{
    private const string TopicPrefix = "whyce.identity.access";

    /// <summary>
    /// Maps an aggregate type to its canonical `.events` topic channel.
    /// All events for a given aggregate type route to the same channel.
    /// </summary>
    public static string MapToTopic(string aggregateType, string eventType)
    {
        var domain = NormalizeDomain(aggregateType);
        return $"{TopicPrefix}.{domain}.events";
    }

    /// <summary>
    /// Returns all canonical topic channels for identity bounded contexts.
    /// Each BC has 4 channels: commands, events, deadletter, retry.
    /// </summary>
    public static IReadOnlyList<string> GetAllTopics() =>
    [
        // Identity
        $"{TopicPrefix}.identity.commands",
        $"{TopicPrefix}.identity.events",
        $"{TopicPrefix}.identity.deadletter",
        $"{TopicPrefix}.identity.retry",

        // Credential
        $"{TopicPrefix}.credential.commands",
        $"{TopicPrefix}.credential.events",
        $"{TopicPrefix}.credential.deadletter",
        $"{TopicPrefix}.credential.retry",

        // Role
        $"{TopicPrefix}.role.commands",
        $"{TopicPrefix}.role.events",
        $"{TopicPrefix}.role.deadletter",
        $"{TopicPrefix}.role.retry",

        // Permission
        $"{TopicPrefix}.permission.commands",
        $"{TopicPrefix}.permission.events",
        $"{TopicPrefix}.permission.deadletter",
        $"{TopicPrefix}.permission.retry",

        // Trust
        $"{TopicPrefix}.trust.commands",
        $"{TopicPrefix}.trust.events",
        $"{TopicPrefix}.trust.deadletter",
        $"{TopicPrefix}.trust.retry",

        // Verification
        $"{TopicPrefix}.verification.commands",
        $"{TopicPrefix}.verification.events",
        $"{TopicPrefix}.verification.deadletter",
        $"{TopicPrefix}.verification.retry",

        // Consent
        $"{TopicPrefix}.consent.commands",
        $"{TopicPrefix}.consent.events",
        $"{TopicPrefix}.consent.deadletter",
        $"{TopicPrefix}.consent.retry",

        // Session
        $"{TopicPrefix}.session.commands",
        $"{TopicPrefix}.session.events",
        $"{TopicPrefix}.session.deadletter",
        $"{TopicPrefix}.session.retry",

        // Device
        $"{TopicPrefix}.device.commands",
        $"{TopicPrefix}.device.events",
        $"{TopicPrefix}.device.deadletter",
        $"{TopicPrefix}.device.retry",

        // ServiceIdentity
        $"{TopicPrefix}.service-identity.commands",
        $"{TopicPrefix}.service-identity.events",
        $"{TopicPrefix}.service-identity.deadletter",
        $"{TopicPrefix}.service-identity.retry",

        // IdentityGraph
        $"{TopicPrefix}.identity-graph.commands",
        $"{TopicPrefix}.identity-graph.events",
        $"{TopicPrefix}.identity-graph.deadletter",
        $"{TopicPrefix}.identity-graph.retry",

        // AccessProfile
        $"{TopicPrefix}.access-profile.commands",
        $"{TopicPrefix}.access-profile.events",
        $"{TopicPrefix}.access-profile.deadletter",
        $"{TopicPrefix}.access-profile.retry",
    ];

    private static string NormalizeDomain(string aggregateType)
    {
        // Convert PascalCase to kebab-case: "IdentityGraph" -> "identity-graph"
        var result = new StringBuilder();
        for (int i = 0; i < aggregateType.Length; i++)
        {
            if (char.IsUpper(aggregateType[i]) && i > 0)
                result.Append('-');
            result.Append(char.ToLowerInvariant(aggregateType[i]));
        }
        return result.ToString();
    }
}
