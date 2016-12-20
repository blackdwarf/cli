// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.DotNet.Cli;
using Microsoft.DotNet.Cli.Utils;
using System.Linq;
using System.IO;

namespace Microsoft.DotNet.Tools.List.ListInstalledRuntimes
{
    internal class ListInstalledRuntimesCommand : DotNetSubCommandBase
    {
        public static DotNetSubCommandBase Create()
        {
            var command = new ListInstalledRuntimesCommand()
            {
                Name = "runtimes",
                FullName = LocalizableStrings.AppFullName,
                Description = LocalizableStrings.AppDescription,
            };

            command.HelpOption("-h|--help");

            return command;
        }

        public override int Run(string fileOrDirectory)
        {
            var muxer = new Muxer();
            var separator = Path.DirectorySeparatorChar;
            var sharedFxPath = $"{Path.GetDirectoryName(muxer.MuxerPath)}{separator}shared{separator}Microsoft.NETCore.App";
            if (Directory.Exists(sharedFxPath))
            {
                Reporter.Output.WriteLine($"Install location: {sharedFxPath}");
                Reporter.Output.WriteLine("Installed versions:");
                foreach (var dir in Directory.EnumerateDirectories(sharedFxPath))
                {
                    Reporter.Output.WriteLine($"  - v{Path.GetFileName(dir)}");
                }
            }
            else
            {
                Reporter.Error.WriteLine("You don't seem to have any shared frameworks installed.");
            }
            
            return 0;
        }
    }
}
