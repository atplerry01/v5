using System.Text.RegularExpressions;
using Xunit;

namespace Whycespace.Tests.Unit.Architecture;

/// <summary>
/// R3.B.1 — grep-based architecture invariants for the outbound-effect
/// subsystem. Enforces the <c>R-OUT-EFF-PROHIBITION-*</c> and
/// <c>R-OUT-EFF-SEAM-*</c> rule families the moment the seam lands —
/// prohibition is law from day one (ratified constraint #5).
/// </summary>
public sealed class OutboundEffectProhibitionTests
{
    private static readonly string RepoRoot = FindRepoRoot();
    private static readonly string SrcRoot = Path.Combine(RepoRoot, "src");

    // Canonical whitelist segments for HttpClient usage. OPA is grandfathered
    // per D-R3B-10 — the adapter files themselves plus the existing OPA
    // composition sites and health check that inject the HttpClient into
    // those adapters. The outbound-effect adapter subtree is the new
    // sanctioned production surface. Shared contracts carry type references
    // for interfaces. Nothing else may use HttpClient.
    private static readonly string[] HttpClientWhitelistFragments =
    {
        // OPA grandfathered family (D-R3B-10)
        Path.Combine("platform", "host", "adapters", "OpaPolicyEvaluator.cs"),
        Path.Combine("platform", "host", "adapters", "OpaWarmupService.cs"),
        Path.Combine("platform", "host", "adapters", "OpaEnforcementEventEvaluator.cs"),
        Path.Combine("platform", "host", "health", "OpaHealthCheck.cs"),
        Path.Combine("platform", "host", "composition", "infrastructure", "policy", "PolicyInfrastructureModule.cs"),
        Path.Combine("platform", "host", "composition", "observability", "ObservabilityComposition.cs"),
        // R3.B.1 sanctioned surfaces
        Path.Combine("platform", "host", "adapters", "outbound-effects"),
        Path.Combine("runtime", "outbound-effects"),
        Path.Combine("shared", "contracts", "infrastructure"),
        // R3.B.2 composition sites that construct HttpClient for sanctioned
        // outbound-effect adapters (parallel to the OPA composition exception).
        Path.Combine("platform", "host", "composition", "integration", "outbound-effect"),
    };

    [Fact]
    public void HttpClient_Usage_Confined_To_Whitelist()
    {
        var forbidden = new Regex(
            @"\b(HttpClient|HttpClientHandler|HttpMessageInvoker|WebClient|SocketsHttpHandler)\b");

        var hits = ScanCode(SrcRoot, forbidden)
            .Where(line => !IsWhitelisted(line))
            .ToList();

        Assert.True(hits.Count == 0,
            "R-OUT-EFF-PROHIBITION-01: HttpClient-family types are forbidden outside " +
            "the sanctioned whitelist (OPA grandfathered + " +
            "src/platform/host/adapters/outbound-effects/** + src/runtime/outbound-effects/** + " +
            "src/shared/contracts/infrastructure/**). Hits:\n" + string.Join("\n", hits));
    }

    [Fact]
    public void Adapter_DispatchAsync_Only_Invoked_From_Relay()
    {
        // The interface method is named DispatchAsync; scan for invocations
        // of that exact method on IOutboundEffectAdapter instances.
        var forbidden = new Regex(@"\badapter(?:Instance|\b)?\.DispatchAsync\s*\(", RegexOptions.IgnoreCase);

        var relayPath = Path.Combine(SrcRoot, "runtime", "outbound-effects", "OutboundEffectRelay.cs");

        var hits = ScanCode(SrcRoot, forbidden)
            .Where(line => !line.StartsWith(relayPath, StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.True(hits.Count == 0,
            "R-OUT-EFF-PROHIBITION-03: IOutboundEffectAdapter.DispatchAsync may only be " +
            "invoked from OutboundEffectRelay. Hits:\n" + string.Join("\n", hits));
    }

    [Fact]
    public void OutboundEffectAggregate_Start_Only_Called_From_Factory()
    {
        var forbidden = new Regex(@"\bOutboundEffectAggregate\.Start\s*\(");

        var factoryPath = Path.Combine(
            SrcRoot, "engines", "T2E", "outbound-effects", "lifecycle",
            "OutboundEffectLifecycleEventFactory.cs");
        var aggregatePath = Path.Combine(
            SrcRoot, "domain", "integration-system", "outbound-effect", "aggregate",
            "OutboundEffectAggregate.cs");

        var hits = ScanCode(SrcRoot, forbidden)
            .Where(line => !line.StartsWith(factoryPath, StringComparison.OrdinalIgnoreCase))
            .Where(line => !line.StartsWith(aggregatePath, StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.True(hits.Count == 0,
            "R-OUT-EFF-SEAM-01: OutboundEffectAggregate.Start may only be called from " +
            "OutboundEffectLifecycleEventFactory. Hits:\n" + string.Join("\n", hits));
    }

    [Fact]
    public void Relay_Backoff_Helper_Is_Replay_Deterministic()
    {
        // R3.B.3 / R-OUT-EFF-BACKOFF-DET-01 — OutboundEffectRelay.ComputeBackoffMs
        // MUST derive its result from only (options, attemptNumber, effectId,
        // providerRetryAfter, IRandomProvider(seed)). No clock reads, no
        // ambient random. Parallel to R-RETRY-DELIVER-AFTER-DETERMINISM-01
        // for the Kafka retry escalator.
        var relayPath = Path.Combine(
            SrcRoot, "runtime", "outbound-effects", "OutboundEffectRelay.cs");
        Assert.True(File.Exists(relayPath), $"relay source missing: {relayPath}");

        // Extract the body of the ComputeBackoffMs method.
        var content = File.ReadAllText(relayPath);
        var start = content.IndexOf("ComputeBackoffMs(", StringComparison.Ordinal);
        Assert.True(start >= 0, "ComputeBackoffMs declaration not found.");

        // Take the next ~2000 chars after the declaration and scan for
        // forbidden tokens. Forbidden tokens include clock/random/entropy
        // reads AND ambient calls that would defeat replay-determinism.
        var body = content.Substring(start, Math.Min(2_000, content.Length - start));
        var forbidden = new Regex(
            @"\b(DateTime(Offset)?\.(UtcNow|Now)|\.Ticks\b|Guid\.NewGuid|new\s+Random|Random\.Shared|Stopwatch\.)");

        Assert.False(forbidden.IsMatch(body),
            "R-OUT-EFF-BACKOFF-DET-01: ComputeBackoffMs must not read clock or ambient random. " +
            "All entropy flows through IRandomProvider.NextDouble(seed). Body scan flagged " +
            "a forbidden token inside the helper.");
    }

    [Fact]
    public void Determinism_In_Outbound_Effect_Code()
    {
        var subtrees = new[]
        {
            Path.Combine(SrcRoot, "runtime", "outbound-effects"),
            Path.Combine(SrcRoot, "engines", "T2E", "outbound-effects"),
            Path.Combine(SrcRoot, "platform", "host", "adapters", "outbound-effects"),
        };

        var forbidden = new Regex(
            @"\b(Guid\.NewGuid|DateTime\.UtcNow|DateTime\.Now|DateTimeOffset\.UtcNow|DateTimeOffset\.Now|new\s+Random|Random\.Shared)\b");

        var hits = new List<string>();
        foreach (var root in subtrees)
        {
            hits.AddRange(ScanCode(root, forbidden));
        }

        Assert.True(hits.Count == 0,
            "R-OUT-EFF-DET-01: outbound-effect subtrees must use IClock / IIdGenerator / " +
            "IRandomProvider only. Guid.NewGuid / DateTime.UtcNow / Random are forbidden. " +
            "Hits:\n" + string.Join("\n", hits));
    }

    [Fact]
    public void NoOp_Adapter_Not_In_Production_Composition()
    {
        var compositionRoots = new[]
        {
            Path.Combine(SrcRoot, "platform", "host", "Program.cs"),
            Path.Combine(SrcRoot, "platform", "host", "composition"),
        };

        var forbidden = new Regex(@"\bNoOpOutboundEffectAdapter\b");

        var hits = new List<string>();
        foreach (var root in compositionRoots)
        {
            if (File.Exists(root))
            {
                var text = File.ReadAllText(root);
                if (forbidden.IsMatch(text))
                    hits.Add(root);
            }
            else if (Directory.Exists(root))
            {
                hits.AddRange(ScanCode(root, forbidden));
            }
        }

        Assert.True(hits.Count == 0,
            "R-OUT-EFF-PROHIBITION-05: NoOpOutboundEffectAdapter must not be referenced " +
            "from production composition (Program.cs / src/platform/host/composition/**). " +
            "Hits:\n" + string.Join("\n", hits));
    }

    [Fact]
    public void Registered_Adapters_Live_In_Sanctioned_Subtree()
    {
        // R3.B.2 — every IOutboundEffectAdapter implementer MUST reside under
        // src/platform/host/adapters/outbound-effects/. Source of truth is
        // file location.
        var implementsPattern = new Regex(
            @"\bclass\s+\w+\s*:.*\bIOutboundEffectAdapter\b|\brecord\s+\w+\s*:.*\bIOutboundEffectAdapter\b");
        var sanctioned = Path.Combine(SrcRoot, "platform", "host", "adapters", "outbound-effects");

        var hits = ScanCode(SrcRoot, implementsPattern)
            .Where(line => !line.StartsWith(sanctioned, StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.True(hits.Count == 0,
            "R-OUT-EFF-PROHIBITION-03: every IOutboundEffectAdapter implementation " +
            "must live under src/platform/host/adapters/outbound-effects/. Hits:\n" +
            string.Join("\n", hits));
    }

    [Fact]
    public void WebhookDeliveryAdapter_Is_Present_And_Sanctioned()
    {
        var canonical = Path.Combine(
            SrcRoot, "platform", "host", "adapters", "outbound-effects",
            "WebhookDeliveryAdapter.cs");

        Assert.True(File.Exists(canonical),
            "R3.B.2: WebhookDeliveryAdapter.cs must exist at " + canonical);

        var leakPattern = new Regex(@"\bWebhookDeliveryAdapter\b");
        var forbiddenSubtrees = new[]
        {
            Path.Combine(SrcRoot, "domain"),
            Path.Combine(SrcRoot, "engines"),
            Path.Combine(SrcRoot, "projections"),
            Path.Combine(SrcRoot, "runtime"),
        };

        var hits = new List<string>();
        foreach (var sub in forbiddenSubtrees)
        {
            hits.AddRange(ScanCode(sub, leakPattern));
        }

        Assert.True(hits.Count == 0,
            "R3.B.2: WebhookDeliveryAdapter must not be referenced from " +
            "domain/engines/projections/runtime. Hits:\n" + string.Join("\n", hits));
    }

    [Fact]
    public void OutboundEffect_Aggregate_And_Events_Under_IntegrationSystem()
    {
        var canonicalAggregatePath = Path.Combine(
            SrcRoot, "domain", "integration-system", "outbound-effect", "aggregate",
            "OutboundEffectAggregate.cs");
        Assert.True(File.Exists(canonicalAggregatePath),
            "DG-R-OUT-EFF-PLACEMENT-01: OutboundEffectAggregate must live at " +
            canonicalAggregatePath);

        var canonicalEventFolder = Path.Combine(
            SrcRoot, "domain", "integration-system", "outbound-effect", "event");

        var eventDefinition = new Regex(@"\bpublic\s+sealed\s+record\s+OutboundEffect\w*Event\b");

        var hitsOutsideCanonical = ScanCode(SrcRoot, eventDefinition)
            .Where(line => !line.StartsWith(canonicalEventFolder, StringComparison.OrdinalIgnoreCase))
            .Where(line => !line.Contains("EventSchemas.cs", StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.True(hitsOutsideCanonical.Count == 0,
            "DG-R-OUT-EFF-PLACEMENT-01: OutboundEffect*Event definitions must live under " +
            "src/domain/integration-system/outbound-effect/event/. Hits:\n" +
            string.Join("\n", hitsOutsideCanonical));
    }

    private static bool IsWhitelisted(string fileLine) =>
        HttpClientWhitelistFragments.Any(fragment =>
            fileLine.Contains(fragment, StringComparison.OrdinalIgnoreCase));

    private static List<string> ScanCode(string root, Regex pattern)
    {
        var hits = new List<string>();
        if (!Directory.Exists(root)) return hits;

        foreach (var file in Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories))
        {
            if (file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") ||
                file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
                continue;

            var lines = File.ReadAllLines(file);
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var commentIdx = line.IndexOf("//", StringComparison.Ordinal);
                var scanned = commentIdx >= 0 ? line.Substring(0, commentIdx) : line;
                scanned = Regex.Replace(scanned, "\"[^\"]*\"", "\"\"");
                if (pattern.IsMatch(scanned))
                    hits.Add($"{file}:{i + 1}: {line.Trim()}");
            }
        }
        return hits;
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            if (Directory.Exists(Path.Combine(dir.FullName, "src")) &&
                Directory.Exists(Path.Combine(dir.FullName, "claude")))
                return dir.FullName;
            dir = dir.Parent;
        }
        throw new DirectoryNotFoundException(
            "Could not locate WBSM repo root from " + AppContext.BaseDirectory);
    }
}
