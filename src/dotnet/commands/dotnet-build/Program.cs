// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.DotNet.Cli.CommandLine;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.DotNet.Tools.MSBuild;
using System.Linq;
using Microsoft.DotNet.Tools.Common;

namespace Microsoft.DotNet.Tools.Build
{
    public class BuildCommand
    {
        private static string GetFileNameFromResourceName(string s)
        {
            // A.B.C.D.filename.extension
            string[] parts = s.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
            {
                return null;
            }

            // filename.extension
            return parts[parts.Length - 2] + "." + parts[parts.Length - 1];
        }


        public int DropDefaultProject(string projectFilePath)
        {
            if (File.Exists(projectFilePath))
            {
                return 0;
            }

            PathUtility.EnsureDirectory(Path.GetDirectoryName(projectFilePath));

            var thisAssembly = typeof(Build3Command).GetTypeInfo().Assembly;
            var resources = from resourceName in thisAssembly.GetManifestResourceNames()
                            where resourceName.Contains("DefaultProject")
                            select resourceName;

            foreach (string resourceName in resources)
            {
                string fileName = GetFileNameFromResourceName(resourceName);

                using (var resource = thisAssembly.GetManifestResourceStream(resourceName))
                {
                    try
                    {

                        using (var file = File.Create(projectFilePath))
                        {
                            resource.CopyTo(file);
                        }
                    } catch (IOException ex)
                    {
                        Reporter.Error.WriteLine(ex.Message);
                        return 1;
                    }
                }
            }

            return 0;
        }
        public static int Run(string[] args)
        {
            DebugHelper.HandleDebugSwitch(ref args);

            CommandLineApplication app = new CommandLineApplication(throwOnUnexpectedArg: false);
            app.Name = "dotnet build";
            app.FullName = ".NET Builder";
            app.Description = "Builder for the .NET Platform. Delegates to the MSBuild 'Build' target in the project file.";
            app.AllowArgumentSeparator = true;
            app.ArgumentSeparatorHelpText = HelpMessageStrings.MSBuildAdditionalArgsHelpText;
            app.HelpOption("-h|--help");

            CommandArgument projectArgument = app.Argument("<PROJECT>",
                "The MSBuild project file to build. If a project file is not specified," +
                " MSBuild searches the current working directory for a file that has a file extension that ends in `proj` and uses that file.");

            CommandOption outputOption = app.Option("-o|--output <OUTPUT_DIR>", "Directory in which to place outputs", CommandOptionType.SingleValue);
            CommandOption frameworkOption = app.Option("-f|--framework <FRAMEWORK>", "Compile a specific framework", CommandOptionType.SingleValue);
            CommandOption runtimeOption = app.Option(
                "-r|--runtime <RUNTIME_IDENTIFIER>", "Target runtime to build for. The default is to build a portable application.",
                CommandOptionType.SingleValue);
            CommandOption configurationOption = app.Option("-c|--configuration <CONFIGURATION>", "Configuration under which to build", CommandOptionType.SingleValue);
            CommandOption versionSuffixOption = app.Option("--version-suffix <VERSION_SUFFIX>", "Defines the value for the $(VersionSuffix) property in the project", CommandOptionType.SingleValue);

            CommandOption noIncrementalOption = app.Option("--no-incremental", "Set this flag to turn off incremental build", CommandOptionType.NoValue);
            CommandOption noDependenciesOption = app.Option("--no-dependencies", "Set this flag to ignore project to project references and only build the root project", CommandOptionType.NoValue);
            CommandOption verbosityOption = MSBuildForwardingApp.AddVerbosityOption(app);
            var build3 = new Build3Command();
            app.OnExecute(() =>
            {
                List<string> msbuildArgs = new List<string>();

                if (!string.IsNullOrEmpty(projectArgument.Value))
                {
                    msbuildArgs.Add(projectArgument.Value);
                }
                else if (!PathUtility.FilesExistInDirectory(Directory.GetCurrentDirectory(), "*.csproj"))
                {
                    var currentDir = Directory.GetCurrentDirectory();
                    var separator = Path.DirectorySeparatorChar;
                    if (PathUtility.FilesExistInDirectory(currentDir, "*.cs"))
                    {
                        var projectFilePath = Path.Combine(Env.GetHomeDirectory(), ".dotnet", "defaultproject.csproj");
                        build3.DropDefaultProject(projectFilePath);

                        // Get the default app name - use the current dir
                        var appName = PathUtility.GetDirectoryName(currentDir);

                        msbuildArgs.Add(projectFilePath);
                        msbuildArgs.Add($"/p:CompileIncludes={currentDir}{separator}**");
                        msbuildArgs.Add($"/p:AssemblyName={appName}");
                        msbuildArgs.Add($"/p:BaseOutputPath={currentDir}{separator}bin{separator}");
                        msbuildArgs.Add($"/p:OutputPath={currentDir}{separator}bin{separator}");
                        msbuildArgs.Add($"/p:BaseIntermediateOutputPath={currentDir}{separator}obj{separator}");
                    }
                    else if (Directory.GetFiles(currentDir, "*.fs").Length > 0)
                    {
                        // We have F# source files
                    }
                }

                if (noIncrementalOption.HasValue())
                {
                    msbuildArgs.Add("/t:Rebuild");
                }
                else
                {
                    msbuildArgs.Add("/t:Build");
                }

                if (outputOption.HasValue())
                {
                    msbuildArgs.Add($"/p:OutputPath={outputOption.Value()}");
                }

                if (frameworkOption.HasValue())
                {
                    msbuildArgs.Add($"/p:TargetFramework={frameworkOption.Value()}");
                }

                if (runtimeOption.HasValue())
                {
                    msbuildArgs.Add($"/p:RuntimeIdentifier={runtimeOption.Value()}");
                }

                if (configurationOption.HasValue())
                {
                    msbuildArgs.Add($"/p:Configuration={configurationOption.Value()}");
                }

                if (versionSuffixOption.HasValue())
                {
                    msbuildArgs.Add($"/p:VersionSuffix={versionSuffixOption.Value()}");
                }

                if (noDependenciesOption.HasValue())
                {
                    msbuildArgs.Add("/p:BuildProjectReferences=false");
                }

                if (verbosityOption.HasValue())
                {
                    msbuildArgs.Add($"/verbosity:{verbosityOption.Value()}");
                }

                msbuildArgs.AddRange(app.RemainingArguments);

                return new MSBuildForwardingApp(msbuildArgs).Execute();
            });

            return app.Execute(args);
        }
    }
}
