using System;
using Microsoft.DotNet.Tools.Add;
using Microsoft.DotNet.Tools.Build;
using Microsoft.DotNet.Tools.Clean;
using Microsoft.DotNet.Tools.Help;
using Microsoft.DotNet.Tools.List;
using Microsoft.DotNet.Tools.Migrate;
using Microsoft.DotNet.Tools.MSBuild;
using Microsoft.DotNet.Tools.New;
using Microsoft.DotNet.Tools.NuGet;
using Microsoft.DotNet.Tools.Pack;
using Microsoft.DotNet.Tools.Publish;
using Microsoft.DotNet.Tools.Remove;
using Microsoft.DotNet.Tools.Restore;
using Microsoft.DotNet.Tools.RestoreProjectJson;
using Microsoft.DotNet.Tools.Run;
using Microsoft.DotNet.Tools.Sln;
using Microsoft.DotNet.Tools.Test;
using Microsoft.DotNet.Tools.VSTest;
using Microsoft.DotNet.Tools.Cache;

namespace Microsoft.DotNet.Cli
{
    public static class BuiltinCommands
    {
        public static Dictionary<string, BuiltinConf> Commands = new Dictionary<string, BuiltinConf>
        {
            ["add"] = new BuiltinConf
            {
                Command = AddCommand.Run,
                DocumentationUrl = new Uri("")
            },
            ["build"] = new BuiltinConf
            {
                Command = BuildCommand.Run,
                DocumentationUrl = new Uri("https://aka.ms/dotnet-build")
            },
            ["cache"] = new BuiltinConf
            {
                Command = CacheCommand.Run,
                DocumentationUrl = new Uri("")
            },
            ["clean"] = new BuiltinConf 
            {
                Command = CleanCommand.Run,
                DocumentationUrl = new Uri("https://aka.ms/dotnet-clean")
            },
            ["help"] = new BuiltinConf
            {
                Command = HelpCommand.Run,
                DocumentationUrl = new Uri("https://aka.ms/dotnet-help")
            },
            ["list"] = new BuiltinConf
            {
                Command = ListCommand.Run,
                DocumentationUrl = new Uri("")
            },
            ["migrate"] = new BuiltinConf
            {
                Command = MigrateCommand.Run,
                DocumentationUrl = new Uri("http://aka.ms/dotnet-migrate")

            },
            ["msbuild"] = new BuiltinConf
            {
                Command = MSBuildCommand.Run,
                DocumentationUrl = new Uri("https://aka.ms/dotnet-msbuild")
            },
            ["new"] = new BuiltinConf
            {
                Command = NewCommandShim.Run,
                DocumentationUrl = new Uri("https://aka.ms/dotnet-new")
            },
            ["nuget"] = new BuiltinConf
            {
                Command = NuGetCommand.Run,
                DocumentationUrl = new Uri("https://aka.ms/dotnet-nuget")
            },
            ["pack"] = new BuiltinConf
            {
                Command = PackCommand.Run,
                DocumentationUrl = new Uri("https://aka.ms/dotnet-pack")
            },
            ["publish"] = new BuiltinConf
            {
                Command = PublishCommand.Run,
                DocumentationUrl = new Uri("https://aka.ms/dotnet-publish")
            },
            ["remove"] = new BuiltinConf
            {
                Command = RemoveCommand.Run,
                DocumentationUrl = new Uri("https://aka.ms/dotnet-remove")
            },
            ["restore"] = new BuiltinConf
            {
                Command = RestoreCommand.Run,
                DocumentationUrl = new Uri("https://aka.ms/dotnet-restore")
            },
            ["run"] = new BuiltinConf
            {
                Command = RunCommand.Run,
                DocumentationUrl = new Uri("https://aka.ms/dotnet-run")
            },
            ["sln"] = new BuiltinConf
            {
                Command = SlnCommand.Run,
                DocumentationUrl = new Uri("https://aka.ms/dotnet-sln")
            },
            ["test"] = new BuiltinConf
            {
                Command = TestCommand.Run,,
                DocumentationUrl = new Uri("https://aka.ms/dotnet-test")
            },
            ["vstest"] = new BuiltinConf
            {
                Command = VSTestCommand.Run,,
                DocumentationUrl = new Uri("https://aka.ms/dotnet-vstest")
            }
        };

    }

    public class BuiltinConf
    {
        public Func<string[], int> Command { get; set; }
        public Uri DocumentationUrl { get; set; }
    }
}