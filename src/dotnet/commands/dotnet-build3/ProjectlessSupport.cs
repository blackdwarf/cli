using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.DotNet.Tools.Common;

namespace Microsoft.DotNet.Tools.Build3
{
    public class ProjectlessSupport
    {

        private readonly string _currentDirectory;
        private string _appName;

        public ProjectlessSupport() : this(Directory.GetCurrentDirectory()) { }

        public ProjectlessSupport(string currentDir)
        {
            _currentDirectory = currentDir;
            _appName = PathUtility.GetDirectoryName(_currentDirectory);
        }


        public bool IsProjectlessWorkspace()
        {
            return (!PathUtility.FilesExistInDirectory(_currentDirectory, "*.csproj") && !PathUtility.FilesExistInDirectory(_currentDirectory, "*.fsproj")
                && !PathUtility.FilesExistInDirectory(_currentDirectory, "*.vbproj") && !PathUtility.FilesExistInDirectory(_currentDirectory, "*.proj"));
        }

        public bool IsCsharpWorkspace()
        {
            return DetermineWorkspace("*.cs");            
        }

        public bool IsFsharpWorkspace()
        {
            throw new NotImplementedException();
        }

        public bool IsVbNetWorkspace()
        {
            throw new NotImplementedException();
        }

        private bool DetermineWorkspace(string glob)
        {
            return PathUtility.FilesExistInDirectory(_currentDirectory, glob);
        }

        public void PackageArgs(ref List<string> msbuildArgs)
        {
            var projectFileName = "defaultproject";
            if (IsCsharpWorkspace())
            {
                projectFileName += ".csproj";
            }
            else if (IsFsharpWorkspace())
            {
                // here is where the F# stuff would come along
                projectFileName += ".fsproj";
            }
            else if (IsVbNetWorkspace())
            {
                projectFileName += ".vbproj";
            }

            var projectFilePath = Path.Combine(Env.GetUserHomeDirectory(), ".dotnet", projectFileName);

            DropDefaultProject(projectFilePath, projectFileName);

            char separator = Path.DirectorySeparatorChar;
            msbuildArgs.Add(projectFilePath);
            msbuildArgs.Add($"/p:CompileIncludes={_currentDirectory}{separator}**");
            msbuildArgs.Add($"/p:AssemblyName={_appName}");
            msbuildArgs.Add($"/p:BaseOutputPath={_currentDirectory}{separator}bin{separator}");
            msbuildArgs.Add($"/p:OutputPath={_currentDirectory}{separator}bin{separator}");
            msbuildArgs.Add($"/p:BaseIntermediateOutputPath={_currentDirectory}{separator}obj{separator}");

        }

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


        private int DropDefaultProject(string projectFilePath, string projectFileName)
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
                if (!string.Equals(fileName, projectFileName))
                    continue;

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

    }
}
