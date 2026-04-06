using System.Security.Cryptography;
using System.Text;

namespace Whycespace.Domain.ConstitutionalSystem.Policy.Rule;

/// <summary>
/// Deterministic compiler: PolicyDefinition → Rego module.
/// One policy → one .rego file. No runtime execution.
/// Returns artifact metadata for version linkage.
/// </summary>
public static class PolicyToRegoCompiler
{
    public static CompilationResult Compile(PolicyDefinition definition, Guid policyId, int versionNumber)
    {
        ArgumentNullException.ThrowIfNull(definition);

        var rego = CompileToRego(definition);
        var hash = ComputeHash(rego);
        var location = $"compiled/{policyId}/{versionNumber}/policy.rego";
        var artifactId = $"{policyId}:{versionNumber}";

        return new CompilationResult(rego, artifactId, hash, location);
    }

    public static string CompileToRego(PolicyDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        var validation = PolicyDslValidator.Validate(definition);
        if (!validation.IsValid)
            throw new InvalidOperationException(
                $"Cannot compile invalid policy: {string.Join("; ", validation.Errors)}");

        var sb = new StringBuilder();
        var packageName = definition.Name.Replace('.', '_').Replace('-', '_').ToLowerInvariant();

        sb.AppendLine($"package whyce.policy.{packageName}");
        sb.AppendLine();
        sb.AppendLine("import future.keywords.in");
        sb.AppendLine();
        sb.AppendLine("default allow = false");
        sb.AppendLine("default deny = false");
        sb.AppendLine();

        foreach (var rule in definition.Rules)
        {
            var regoBlock = rule.Decision.ToUpperInvariant() switch
            {
                "ALLOW" => "allow",
                "DENY" => "deny",
                _ => throw new InvalidOperationException($"Unknown decision: {rule.Decision}")
            };

            sb.AppendLine($"# Rule: {rule.Name}");
            sb.AppendLine($"{regoBlock} {{");

            foreach (var constraint in rule.Constraints)
            {
                var regoExpr = CompileConstraint(constraint);
                sb.AppendLine($"    {regoExpr}");
            }

            sb.AppendLine("}");
            sb.AppendLine();
        }

        // Enriched result output — rules, violations, trace
        sb.AppendLine("# Enriched result output");
        sb.AppendLine("rules[entry] {");
        sb.AppendLine("    some rule_name");
        sb.AppendLine("    entry := {\"rule\": rule_name, \"result\": \"evaluated\"}");
        sb.AppendLine("}");
        sb.AppendLine();

        sb.AppendLine("violations[entry] {");
        sb.AppendLine("    deny");
        sb.AppendLine("    entry := {\"rule\": \"deny_rule\", \"reason\": \"constraint_failed\"}");
        sb.AppendLine("}");
        sb.AppendLine();

        sb.AppendLine("trace := {");
        sb.AppendLine("    \"evaluated_rules\": rules,");
        sb.AppendLine("    \"matched_constraints\": violations,");
        sb.AppendLine("}");
        sb.AppendLine();

        sb.AppendLine("# Decision resolution");
        sb.AppendLine("decision = \"DENY\" { deny }");
        sb.AppendLine("decision = \"ALLOW\" { allow; not deny }");
        sb.AppendLine("decision = \"DENY\" { not allow; not deny }");
        sb.AppendLine();

        sb.AppendLine("# Structured result");
        sb.AppendLine("result := {");
        sb.AppendLine("    \"decision\": decision,");
        sb.AppendLine("    \"rules\": rules,");
        sb.AppendLine("    \"violations\": violations,");
        sb.AppendLine("    \"trace\": trace,");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private static string CompileConstraint(ConstraintDefinition constraint)
    {
        var left = CompileOperand(constraint.Left);
        var right = CompileOperand(constraint.Right);
        var op = CompileOperator(constraint.Operator);

        return $"{left} {op} {right}";
    }

    private static string CompileOperand(string operand)
    {
        if (operand.Contains('.', StringComparison.Ordinal))
            return $"input.{operand}";

        if (int.TryParse(operand, out _) || double.TryParse(operand, out _))
            return operand;

        return $"\"{operand}\"";
    }

    private static string CompileOperator(string op) => op switch
    {
        "eq" => "==",
        "neq" => "!=",
        "gt" => ">",
        "gte" => ">=",
        "lt" => "<",
        "lte" => "<=",
        "contains" => "contains",
        "in" => "in",
        _ => "=="
    };

    private static string ComputeHash(string content)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(content));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}

public sealed record CompilationResult(
    string Rego,
    string ArtifactId,
    string ArtifactHash,
    string ArtifactLocation);
