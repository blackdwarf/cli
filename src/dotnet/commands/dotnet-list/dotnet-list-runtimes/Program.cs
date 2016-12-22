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
                Reporter.Output.WriteLine($"Microsoft.NETCore.App @ {sharedFxPath}:");
                foreach (var dir in Directory.EnumerateDirectories(sharedFxPath))
                {
                    var version = Path.GetFileName(dir);
                    var train = (int.Parse(version.Split('.')[1]) > 0 ? "Current" : "LTS");
                    Reporter.Output.WriteLine($"  - v{version} ({train})");
                }
            }
            else
            {
                Reporter.Error.WriteLine(LocalizableStrings.NoRuntimesError);
            }
            
            return 0;
        }
    }
}
