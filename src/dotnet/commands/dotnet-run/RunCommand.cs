// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Execution;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.DotNet.Tools.MSBuild;
using Microsoft.DotNet.Tools.Build;

namespace Microsoft.DotNet.Tools.Run
{
    public partial class RunCommand
    {
        public string Configuration { get; set; }
        public string Framework { get; set; }
        public string Project { get; set; }
        public IReadOnlyList<string> Args { get; set; }
        private ProjectlessSupport _projectless;
        private bool _isProjectless = false;

        private List<string> _args;

        public RunCommand()
        {
        }

        public int Start()
        {
            Initialize();

            EnsureProjectIsBuilt();

            ICommand runCommand = GetRunCommand();

            return runCommand
                .Execute()
                .ExitCode;
        }

        private void EnsureProjectIsBuilt()
        {
            List<string> buildArgs = new List<string>();
 
            //buildArgs.Add(Project); 
            if (_isProjectless)
            {
                _projectless.PackageArgs(ref buildArgs);
                var restoreArgs = new List<string>() {
                    "/t:Restore",
                    "/verbosity:quiet"  
                };
                restoreArgs.AddRange(buildArgs);
                var restoreOp = new MSBuildForwardingApp(restoreArgs).Execute();
            }
            else
            {
                buildArgs.Add(Project);
            }
            
            buildArgs.Add("/nologo");
            buildArgs.Add("/verbosity:quiet");
            Console.WriteLine($"DEBUG: buildArgs: {string.Join(",", buildArgs)}");

            if (!string.IsNullOrWhiteSpace(Configuration))
            {
                buildArgs.Add($"/p:Configuration={Configuration}");
            }

            if (!string.IsNullOrWhiteSpace(Framework))
            {
                buildArgs.Add($"/p:TargetFramework={Framework}");
            }
            
            var buildResult = new MSBuildForwardingApp(buildArgs).Execute();

            if (buildResult != 0)
            {
                Reporter.Error.WriteLine();
                throw new GracefulException("The build failed. Please fix the build errors and run again.");
            }
        }

        private ICommand GetRunCommand()
        {
            Dictionary<string, string> globalProperties = new Dictionary<string, string>()
            {
                { "MSBuildExtensionsPath", AppContext.BaseDirectory }
            };

            if (!string.IsNullOrWhiteSpace(Configuration))
            {
                globalProperties.Add("Configuration", Configuration);
            }

            if (!string.IsNullOrWhiteSpace(Framework))
            {
                globalProperties.Add("TargetFramework", Framework);
            }
            globalProperties.Add("BaseIntermidiateOutputPath", Directory.GetCurrentDirectory() + "\\obj");

            ProjectInstance projectInstance = new ProjectInstance(Project, globalProperties, null);
            Console.WriteLine($"DEBUG: project instance is: {projectInstance}");
            Console.WriteLine($"DEBUG: fill path is {projectInstance.FullPath}");
            Console.WriteLine($"DEBUG: directory path is {projectInstance.Directory}");

            string runProgram = projectInstance.GetPropertyValue("RunCommand");
            Console.WriteLine($"DEBUG: Run Program is: {runProgram}");
            if (string.IsNullOrEmpty(runProgram))
            {
                string outputType = projectInstance.GetPropertyValue("OutputType");

                throw new GracefulException(string.Join(Environment.NewLine,
                    "Unable to run your project.",
                    "Please ensure you have a runnable project type and ensure 'dotnet run' supports this project.",
                    $"The current OutputType is '{outputType}'."));
            }

            string runArguments = projectInstance.GetPropertyValue("RunArguments");
            string runWorkingDirectory = projectInstance.GetPropertyValue("RunWorkingDirectory");

            string fullArguments = runArguments;
            if (_args.Any())
            {
                fullArguments += " " + ArgumentEscaper.EscapeAndConcatenateArgArrayForProcessStart(_args);
            }

            CommandSpec commandSpec = new CommandSpec(runProgram, fullArguments, CommandResolutionStrategy.None);

            return Command.Create(commandSpec)
                .WorkingDirectory(runWorkingDirectory);
        }

        private void Initialize()
        {
            _projectless = new ProjectlessSupport();
            if (string.IsNullOrWhiteSpace(Project))
            {
                string directory = Directory.GetCurrentDirectory();
                string[] projectFiles = Directory.GetFiles(directory, "*.*proj");

                if (projectFiles.Length == 0)
                {
                    if (_projectless.IsProjectlessWorkspace())
                    {
                        var blah = _projectless.DropDefaultProject();
                        Project = blah;
                        _isProjectless = true;
                    }
                    else
                    {
                    throw new InvalidOperationException(
                        $"Couldn't find a project to run. Ensure a project exists in {directory}." + Environment.NewLine +
                        "Or pass the path to the project using --project");
                    }

                }
                else if (projectFiles.Length > 1)
                {
                    throw new InvalidOperationException(
                        $"Specify which project file to use because this '{directory}' contains more than one project file.");
                }
                else
                {
                    Project = projectFiles[0];
                }
            }

            if (Args == null)
            {
                _args = new List<string>();
            }
            else
            {
                _args = new List<string>(Args);
            }
        }
    }
}
