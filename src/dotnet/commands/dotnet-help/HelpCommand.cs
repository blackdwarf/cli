// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.DotNet.Cli.CommandLine;
using Microsoft.DotNet.Cli.Utils;

namespace Microsoft.DotNet.Tools.Help
{
    public class HelpCommand
    {
        private static readonly string UsageText = $@"{LocalizableStrings.Usage}: dotnet [host-options] [command] [arguments] [common-options]

{LocalizableStrings.Arguments}:
  [command]             {LocalizableStrings.CommandDefinition}
  [arguments]           {LocalizableStrings.ArgumentsDefinition}
  [host-options]        {LocalizableStrings.HostOptionsDefinition}
  [common-options]      {LocalizableStrings.OptionsDescription}

{LocalizableStrings.CommonOptions}:
  -v|--verbose          {LocalizableStrings.VerboseDefinition}
  -h|--help             {LocalizableStrings.HelpDefinition} 

{LocalizableStrings.HostOptions}:
  -d|--diagnostics      {LocalizableStrings.DiagnosticsDefinition}
  --version             {LocalizableStrings.VersionDescription}
  --info                {LocalizableStrings.InfoDescription}

{LocalizableStrings.Commands}:
  new           {LocalizableStrings.NewDefinition}
  restore       {LocalizableStrings.RestoreDefinition}
  build         {LocalizableStrings.BuildDefinition}
  publish       {LocalizableStrings.PublishDefinition}
  run           {LocalizableStrings.RunDefinition}
  test          {LocalizableStrings.TestDefinition}
  pack          {LocalizableStrings.PackDefinition}
  migrate       {LocalizableStrings.MigrateDefinition}
  clean         {LocalizableStrings.CleanDefinition}
  sln           {LocalizableStrings.SlnDefinition}

Project modification commands:
  add           Add items to the project
  remove        Remove items from the project
  list          List items in the project

{LocalizableStrings.AdvancedCommands}:
  nuget         {LocalizableStrings.NugetDefinition}
  msbuild       {LocalizableStrings.MsBuildDefinition}
  vstest        {LocalizableStrings.VsTestDefinition}";

        public static int Run(string[] args)
        {

            CommandLineApplication app = new CommandLineApplication(throwOnUnexpectedArg: false);
            app.Name = "dotnet help";
            // app.FullName = LocalizableStrings.AppFullName;
            // app.Description = LocalizableStrings.AppDescription;
            // app.ArgumentSeparatorHelpText = HelpMessageStrings.MSBuildAdditionalArgsHelpText;
            // app.HandleRemainingArguments = true;            
            // app.HelpOption("-h|--help");

            CommandArgument commandNameArgument = app.Argument("<COMMAND_NAME", "The command for which to get teh help!!!");

            app.OnExecute(() => 
            {
                Cli.BuiltinConf builtIn;
                if (Cli.BuiltinCommands.Commands.TryGetValue(commandNameArgument.Value, out builtIn))
                {
                    // Reporter.Output.WriteLine(builtIn.DocumentationUrl.ToString());
                    ProcessStartInfo psInfo;
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        psInfo = new ProcessStartInfo
                        {
                            FileName = "cmd",
                            Arguments = $"/c start {builtIn.DocumentationUrl.ToString()}"
                        };
                    }
                    else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    {
                        psInfo = new ProcessStartInfo
                        {
                            FileName = "open",
                            Arguments = builtIn.DocumentationUrl.ToString()
                        };
                    }
                    else
                    {
                        psInfo = new ProcessStartInfo
                        {
                            FileName = "xdg-open",
                            Arguments = builtIn.DocumentationUrl.ToString()
                        };
                    }

                    var p = Process.Start(psInfo);
                    p.WaitForExit();

                }
                return 0;
            });
            
            if (args.Length == 0)
            {
                PrintHelp();
                return 0;
            }
            else
            {
                // return Cli.Program.Main(new[] { args[0], "--help" });
                return app.Execute(args);
            }
        }

        public static void PrintHelp()
        {
            PrintVersionHeader();
            Reporter.Output.WriteLine(UsageText);
        }

        public static void PrintVersionHeader()
        {
            var versionString = string.IsNullOrEmpty(Product.Version) ?
                string.Empty :
                $" ({Product.Version})";
            Reporter.Output.WriteLine(Product.LongName + versionString);
        }
    }
}
