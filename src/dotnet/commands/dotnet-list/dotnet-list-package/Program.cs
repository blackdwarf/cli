// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Build.Evaluation;
using Microsoft.DotNet.Cli;
using Microsoft.DotNet.Cli.Utils;
using System.Linq;

namespace Microsoft.DotNet.Tools.List.PackageReferences
{
    internal class ListPackageReferencesCommand : DotNetSubCommandBase
    {
        public static DotNetSubCommandBase Create()
        {
            var command = new ListPackageReferencesCommand()
            {
                Name = "package",
                FullName = LocalizableStrings.AppFullName,
                Description = LocalizableStrings.AppDescription,
            };

            command.HelpOption("-h|--help");

            return command;
        }

        public override int Run(string fileOrDirectory)
        {
            var msbuildProj = MsbuildProject.FromFileOrDirectory(new ProjectCollection(), fileOrDirectory);

            var packageRefs = msbuildProj.GetPackageReferences();
            if (!packageRefs.Any())
            {
                Reporter.Output.WriteLine(string.Format(
                    CommonLocalizableStrings.NoPkgRefencesFound,
                    CommonLocalizableStrings.PkgRef,
                    fileOrDirectory));
                return 0;
            }

            Reporter.Output.WriteLine($"{CommonLocalizableStrings.PackageReferenceOneOrMore, -50} {CommonLocalizableStrings.PackageReferenceVersion, -20}");
            Reporter.Output.WriteLine($"{new string('-', CommonLocalizableStrings.PackageReferenceOneOrMore.Length), -50} {new string('-', CommonLocalizableStrings.PackageReferenceVersion.Length), -20}");
            foreach (var pkg in packageRefs)
            {
                var metadata = pkg.Metadata.SingleOrDefault(pm => pm.Name == "Version");
                Reporter.Output.WriteLine($"{pkg.Include, -50} {metadata.Value, -20}");
            }

            return 0;
        }
    }
}
