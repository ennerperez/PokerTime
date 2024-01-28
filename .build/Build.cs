using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using Serilog;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
// ReSharper disable UnusedMember.Local
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode
    public static int Main() => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)", Name = "configuration")] public readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;
    [Parameter("Environment to build - Default is 'Development' (local) or 'Production' (server)", Name = "environment")] public readonly Environment Environment = IsLocalBuild ? Environment.Development : Environment.Production;

    [Solution] public readonly Solution Solution;

    static AbsolutePath SourceDirectory => RootDirectory / "src";

    static AbsolutePath TestsDirectory => RootDirectory / "tests";

    static AbsolutePath PublishDirectory => RootDirectory / "publish";

    static AbsolutePath ArtifactsDirectory => RootDirectory / "output";

    static AbsolutePath TestResultsDirectory => RootDirectory / "tests" / "results";

    static AbsolutePath ScriptsDirectory => RootDirectory / "scripts";

    Version _version = new("1.0.0.0");
    string _hash = string.Empty;
    readonly string[] _publishProjects = new[]
    {
        "PokerTime.Web"
    };
    readonly string[] _testsProjects = new[]
    {
        "PokerTime.Application.Tests.Unit", "PokerTime.Domain.Tests.Unit", "PokerTime.Web.Tests.Integration", "PokerTime.Web.Tests.Unit"
    };

    Target Prepare => d => d
        .Before(Compile)
        .Executes(() =>
        {
            var assemblyInfoVersionFile = Path.Combine(SourceDirectory, ".files", "AssemblyInfo.Version.cs");
            if (File.Exists(assemblyInfoVersionFile))
            {
                Log.Information("Patching: {File}", assemblyInfoVersionFile);

                using (var gitTag = new Process())
                {
                    gitTag.StartInfo = new ProcessStartInfo("git", "tag --sort=-v:refname")
                    {
                        WorkingDirectory = SourceDirectory, RedirectStandardOutput = true, UseShellExecute = false
                    };
                    gitTag.Start();
                    var value = gitTag.StandardOutput.ReadToEnd().Trim();
                    value = new Regex(@"((?:[0-9]{1,}\.{0,}){1,})", RegexOptions.Compiled).Match(value).Captures.LastOrDefault()?.Value;
                    if (value != null)
                    {
                        _version = Version.Parse(value);
                    }

                    gitTag.WaitForExit();
                }

                using (var gitLog = new Process())
                {
                    gitLog.StartInfo = new ProcessStartInfo("git", "rev-parse --verify HEAD")
                    {
                        WorkingDirectory = SourceDirectory, RedirectStandardOutput = true, UseShellExecute = false
                    };
                    gitLog.Start();
                    _hash = gitLog.StandardOutput.ReadLine()?.Trim().Split(" ", StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
                    gitLog.WaitForExit();
                }

                if (_version != null)
                {
                    var content = File.ReadAllText(assemblyInfoVersionFile);
                    var assemblyVersionRegEx = new Regex(@"\[assembly: AssemblyVersion\(.*\)\]", RegexOptions.Compiled);
                    var assemblyFileVersionRegEx = new Regex(@"\[assembly: AssemblyFileVersion\(.*\)\]", RegexOptions.Compiled);
                    var assemblyInformationalVersionRegEx = new Regex(@"\[assembly: AssemblyInformationalVersion\(.*\)\]", RegexOptions.Compiled);

                    content = assemblyVersionRegEx.Replace(content, $"[assembly: AssemblyVersion(\"{_version}\")]");
                    content = assemblyFileVersionRegEx.Replace(content, $"[assembly: AssemblyFileVersion(\"{_version}\")]");
                    content = assemblyInformationalVersionRegEx.Replace(content, $"[assembly: AssemblyInformationalVersion(\"{_version:3}+{_hash}\")]");

                    File.WriteAllText(assemblyInfoVersionFile, content);

                    Log.Information("Version: {Version}", _version);
                    Log.Information("Hash: {Hash}", _hash);
                }
                else
                {
                    Log.Warning("Version was not found");
                }
            }
        });

    Target Clean => d => d
        .Before(Restore)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach((path) => path.DeleteDirectory());
            TestsDirectory.GlobDirectories("**/bin", "**/obj").ForEach((path) => path.DeleteDirectory());
            AbsolutePath.Create(PublishDirectory).CreateOrCleanDirectory();
            AbsolutePath.Create(ArtifactsDirectory).CreateOrCleanDirectory();
            AbsolutePath.Create(TestResultsDirectory).CreateOrCleanDirectory();
            AbsolutePath.Create(ScriptsDirectory).CreateOrCleanDirectory();
        });

    Target Restore => d => d
        .Executes(() =>
        {
            DotNetToolRestore();
            DotNetRestore(s => s
                .SetProjectFile(Solution));
        });

    Target Compile => d => d
        .DependsOn(Clean)
        .DependsOn(Restore)
        .DependsOn(Prepare)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .EnableNoRestore());
        });

    Target Test => d => d
        .DependsOn(Compile)
        .Executes(() =>
        {
            var testCombinations =
                from project in _testsProjects.Select(m => Solution.AllProjects.FirstOrDefault(o => o.Name == m))
                from framework in project.GetTargetFrameworks()
                select new
                {
                    project, framework
                };

            DotNetTest(s => s
                .EnableNoRestore()
                .EnableNoBuild()
                .SetConfiguration(Configuration)
                .When(true, x => x
                    .SetLoggers("trx")
                    .SetResultsDirectory(TestResultsDirectory))
                .CombineWith(testCombinations, (x, v) => x
                    .SetProjectFile(v.project.Path)
                    .SetFramework(v.framework)));
        });

    Target Publish => d => d
        .DependsOn(Test)
        .DependsOn(Compile)
        .DependsOn(Clean)
        .Executes(() =>
        {
            var publishCombinations =
                from project in _publishProjects.Select(m => Solution.AllProjects.FirstOrDefault(p => p.Name == m))
                from framework in project.GetTargetFrameworks()
                select new
                {
                    project, framework
                };

            DotNetPublish(s => s
                .EnableNoRestore()
                .EnableNoBuild()
                .SetConfiguration(Configuration)
                .DisablePublishSingleFile()
                .CombineWith(publishCombinations, (x, v) => x
                    .SetProject(v.project)
                    .SetFramework(v.framework)
                    .SetOutput($"{PublishDirectory}/{v.project.Name}")));
        });

    Target Pack => d => d
        .DependsOn(Publish)
        .Executes(() =>
        {
            CopyDirectoryRecursively(ScriptsDirectory, $"{ArtifactsDirectory}/Scripts");
            CopyDirectoryRecursively(TestResultsDirectory, $"{ArtifactsDirectory}/Tests");
            foreach (var project in _publishProjects)
            {
                ZipFile.CreateFromDirectory($"{PublishDirectory}/{project}", $"{ArtifactsDirectory}/{project}.zip");
            }

            AbsolutePath.Create(PublishDirectory).CreateOrCleanDirectory();
            AbsolutePath.Create(TestResultsDirectory).CreateOrCleanDirectory();
            AbsolutePath.Create(ScriptsDirectory).CreateOrCleanDirectory();
            Log.Information($"Output: {ArtifactsDirectory}");
        });

}
