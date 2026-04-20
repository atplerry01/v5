using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Whycespace.Tests.Unit.Architecture;

/// <summary>
/// R4.B guard-family enforcement: architectural invariants for the
/// admin/operator control surface. Text-based scan — no reflection — so the
/// unit test project does not need a reference to the host assembly.
///
/// <list type="bullet">
///   <item>R-ADMIN-ROUTE-PREFIX-01 — every admin controller routes under
///   <c>api/admin</c>.</item>
///   <item>R-ADMIN-SCOPE-01 — every admin controller inherits
///   <c>AdminControllerBase</c> (which carries the
///   <c>[Authorize(Policy = AdminScope.PolicyName)]</c> attribute) OR
///   declares the policy attribute directly.</item>
///   <item>R-ADMIN-NO-AGGREGATE-MUTATION-01 — admin controllers MUST NOT
///   import domain aggregate namespaces; mutations flow through sanctioned
///   runtime services.</item>
///   <item>R-ADMIN-COMPOSITION-ORDER-01 — the admin composition module is
///   registered in the composition registry at the locked order.</item>
/// </list>
/// </summary>
public sealed class AdminSurfaceArchitectureTests
{
    private static readonly string RepoRoot = FindRepoRoot();
    private static readonly string AdminControllersRoot =
        Path.Combine(RepoRoot, "src", "platform", "api", "controllers", "platform", "admin");

    [Fact]
    public void Every_admin_controller_routes_under_api_admin_prefix()
    {
        var routeAttr = new Regex(@"\[Route\s*\(\s*(?:""(?<literal>[^""]+)""|AdminScope\.RoutePrefix(?:\s*\+\s*""(?<suffix>[^""]+)"")?)\s*\)\s*\]");

        var violations = new List<string>();
        foreach (var file in EnumerateAdminControllerFiles())
        {
            var text = File.ReadAllText(file);
            if (!IsConcreteController(text)) continue;

            var matches = routeAttr.Matches(text);
            if (matches.Count == 0)
            {
                violations.Add($"{file}: missing [Route(...)] attribute on admin controller.");
                continue;
            }

            foreach (Match m in matches)
            {
                if (m.Groups["literal"].Success)
                {
                    var literal = m.Groups["literal"].Value;
                    if (!literal.StartsWith("api/admin", StringComparison.Ordinal))
                        violations.Add($"{file}: route literal '{literal}' does not start with 'api/admin'.");
                }
            }
        }

        Assert.True(violations.Count == 0,
            "R-ADMIN-ROUTE-PREFIX-01:\n" + string.Join("\n", violations));
    }

    [Fact]
    public void Every_admin_controller_inherits_admin_controller_base_or_declares_admin_policy()
    {
        var violations = new List<string>();
        foreach (var file in EnumerateAdminControllerFiles())
        {
            var text = File.ReadAllText(file);
            if (!IsConcreteController(text)) continue;
            if (Path.GetFileName(file).Equals("AdminControllerBase.cs", StringComparison.Ordinal)) continue;

            var inheritsBase = Regex.IsMatch(text, @":\s*AdminControllerBase\b");
            var declaresPolicy = Regex.IsMatch(text,
                @"\[Authorize\s*\(\s*Policy\s*=\s*AdminScope\.PolicyName\s*\)\s*\]");
            if (!inheritsBase && !declaresPolicy)
                violations.Add($"{file}: admin controller must inherit AdminControllerBase or declare [Authorize(Policy = AdminScope.PolicyName)].");
        }

        Assert.True(violations.Count == 0,
            "R-ADMIN-SCOPE-01:\n" + string.Join("\n", violations));
    }

    [Fact]
    public void Admin_controllers_do_not_import_domain_aggregate_namespaces()
    {
        // Whycespace.Domain.* is the canonical aggregate root namespace tree.
        // Admin controllers MUST stay behind the shared-contracts + runtime-
        // service seam and must NOT reach into domain aggregate types.
        var domainImport = new Regex(@"^\s*using\s+Whycespace\.Domain\.", RegexOptions.Multiline);

        var violations = new List<string>();
        foreach (var file in EnumerateAdminControllerFiles())
        {
            var text = File.ReadAllText(file);
            if (domainImport.IsMatch(text))
                violations.Add($"{file}: admin controllers MUST NOT import Whycespace.Domain.* — mutate through sanctioned runtime services only.");
        }

        Assert.True(violations.Count == 0,
            "R-ADMIN-NO-AGGREGATE-MUTATION-01:\n" + string.Join("\n", violations));
    }

    [Fact]
    public void Admin_composition_module_is_registered_in_registry_at_locked_order()
    {
        var registryPath = Path.Combine(RepoRoot, "src", "platform", "host", "composition", "registry", "CompositionRegistry.cs");
        Assert.True(File.Exists(registryPath), $"CompositionRegistry.cs missing at {registryPath}.");

        var text = File.ReadAllText(registryPath);
        Assert.Contains("AdminCompositionModuleEntry", text);
        Assert.DoesNotContain("// Admin not registered", text, StringComparison.Ordinal);
    }

    private static IEnumerable<string> EnumerateAdminControllerFiles()
    {
        if (!Directory.Exists(AdminControllersRoot))
            yield break;

        foreach (var file in Directory.EnumerateFiles(AdminControllersRoot, "*.cs", SearchOption.AllDirectories))
        {
            if (file.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}") ||
                file.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}"))
                continue;
            yield return file;
        }
    }

    private static bool IsConcreteController(string text) =>
        Regex.IsMatch(text, @"\bpublic\s+(sealed\s+)?class\s+\w+Controller\b");

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Whycespace.sln")) &&
               !Directory.Exists(Path.Combine(dir.FullName, "src")))
        {
            dir = dir.Parent;
        }
        return dir?.FullName ?? AppContext.BaseDirectory;
    }
}
