using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Whycespace.Tests.Unit.Certification;

/// <summary>
/// R5.B / R-CHAOS-HANDLER-COVERAGE-01 — verifies that every failure mode
/// with a declared <c>ExceptionHandler</c>:
///
/// <list type="bullet">
///   <item>has a file at <c>src/platform/api/middleware/{Handler}.cs</c>,</item>
///   <item>declares a <c>TypeUri</c> literal matching the cataloged value,</item>
///   <item>writes <c>httpContext.Response.StatusCode = StatusCodes.Status{N}...</c>
///   matching the cataloged HTTP status,</item>
///   <item>is registered in <c>src/platform/host/Program.cs</c> via
///   <c>AddExceptionHandler&lt;{Handler}&gt;()</c>.</item>
/// </list>
///
/// These invariants are the mechanical contract that R4.A alerts rely on:
/// without them, a canonical fault can ship to prod without a canonical
/// HTTP response, silently breaking the operator feedback loop.
/// </summary>
public sealed class CanonicalHandlerCoverageTests
{
    private static readonly string RepoRoot = FindRepoRoot();
    private static readonly string HandlersDir = Path.Combine(
        RepoRoot, "src", "platform", "api", "middleware");
    private static readonly string ProgramPath = Path.Combine(
        RepoRoot, "src", "platform", "host", "Program.cs");

    [Fact]
    public void Every_handler_file_exists_in_canonical_middleware_directory()
    {
        var violations = new List<string>();
        foreach (var fm in CanonicalFailureModes.All.Where(m => m.ExceptionHandler is not null))
        {
            var path = Path.Combine(HandlersDir, $"{fm.ExceptionHandler}.cs");
            if (!File.Exists(path))
                violations.Add($"{fm.Id}: handler file missing at {path}.");
        }
        Assert.True(violations.Count == 0, string.Join("\n", violations));
    }

    [Fact]
    public void Every_handler_declares_expected_type_uri()
    {
        var violations = new List<string>();
        foreach (var fm in CanonicalFailureModes.All
                     .Where(m => m.ExceptionHandler is not null && m.TypeUri is not null))
        {
            var path = Path.Combine(HandlersDir, $"{fm.ExceptionHandler}.cs");
            var text = File.ReadAllText(path);
            if (!text.Contains($"\"{fm.TypeUri}\"", StringComparison.Ordinal))
                violations.Add($"{fm.Id}: handler does not declare TypeUri '{fm.TypeUri}'.");
        }
        Assert.True(violations.Count == 0, string.Join("\n", violations));
    }

    [Fact]
    public void Every_handler_writes_expected_status_code()
    {
        var violations = new List<string>();
        foreach (var fm in CanonicalFailureModes.All
                     .Where(m => m.ExceptionHandler is not null && m.HttpStatus is not null))
        {
            var path = Path.Combine(HandlersDir, $"{fm.ExceptionHandler}.cs");
            var text = File.ReadAllText(path);
            var expectedSymbol = fm.HttpStatus switch
            {
                400 => "Status400BadRequest",
                409 => "Status409Conflict",
                503 => "Status503ServiceUnavailable",
                _ => throw new InvalidOperationException($"Unexpected status {fm.HttpStatus} for {fm.Id}."),
            };
            // Look for an actual assignment to Response.StatusCode — the
            // ProblemDetails.Status literal is a separate line so we verify
            // the runtime write path specifically.
            var write = new Regex($@"Response\.StatusCode\s*=\s*StatusCodes\.{expectedSymbol}");
            if (!write.IsMatch(text))
                violations.Add($"{fm.Id}: handler {fm.ExceptionHandler} does not write Response.StatusCode = StatusCodes.{expectedSymbol}.");
        }
        Assert.True(violations.Count == 0, string.Join("\n", violations));
    }

    [Fact]
    public void Every_handler_is_registered_in_program_cs()
    {
        Assert.True(File.Exists(ProgramPath), $"Program.cs missing at {ProgramPath}.");
        var program = File.ReadAllText(ProgramPath);

        var violations = new List<string>();
        foreach (var fm in CanonicalFailureModes.All.Where(m => m.ExceptionHandler is not null))
        {
            var registration = $"AddExceptionHandler<{fm.ExceptionHandler}>";
            if (!program.Contains(registration, StringComparison.Ordinal))
                violations.Add($"{fm.Id}: Program.cs does not call {registration}().");
        }
        Assert.True(violations.Count == 0, string.Join("\n", violations));
    }

    [Fact]
    public void No_orphan_handler_files_without_a_failure_mode_entry()
    {
        if (!Directory.Exists(HandlersDir)) return;

        var knownHandlers = CanonicalFailureModes.All
            .Where(m => m.ExceptionHandler is not null)
            .Select(m => m.ExceptionHandler!)
            .ToHashSet();

        var violations = new List<string>();
        foreach (var file in Directory.EnumerateFiles(HandlersDir, "*ExceptionHandler.cs"))
        {
            var name = Path.GetFileNameWithoutExtension(file);
            if (!knownHandlers.Contains(name))
                violations.Add($"{file}: orphan handler — add a failure-mode entry to runtime-failure-modes.yml + CanonicalFailureModes.cs.");
        }
        Assert.True(violations.Count == 0, string.Join("\n", violations));
    }

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
