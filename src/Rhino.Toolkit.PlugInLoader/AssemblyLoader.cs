using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rhino.Toolkit.PlugInLoader
{
    public class AssemblyLoader
    {
        private List<string> OriginalFolders { get; set; }

        private static string DotNetDir { get; set; }

        public string TempFolder{get;set;}

        static AssemblyLoader()
        {
            DotNetDir = string.Concat(Environment.GetEnvironmentVariable("windir"), "\\Microsoft.NET\\Framework\\v2.0.50727");
        }

        public AssemblyLoader()
        {
            TempFolder = string.Empty;
            OriginalFolders = new List<string>();
        }

        private Assembly CopyAndLoadAddin(string srcFilePath, string destFolderPath, bool onlyCopyRelated)
        {
            string destFilePath = string.Empty;
            if (!FileUtils.FileExistsInFolder(srcFilePath, TempFolder))
            {
                string directoryName = Path.GetDirectoryName(srcFilePath);
                if (!OriginalFolders.Contains(directoryName))
                {
                    OriginalFolders.Add(directoryName);
                }
                List<FileInfo> fileInfos = new List<FileInfo>();
                destFilePath = FileUtils.CopyFileToFolder(srcFilePath, destFolderPath, onlyCopyRelated, fileInfos);
                if (string.IsNullOrEmpty(destFilePath))
                {
                    return null;
                }
            }
            return LoadAddin(destFilePath);
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            Assembly assembly;
            lock (this)
            {
                string[] strArrays = args.Name.Split(new char[] { ',' });

                string assemblyName = strArrays[0];
                string language = strArrays[2];

                string foundFilePath = SearchAssemblyFileInTempFolder(assemblyName);
                if (!File.Exists(foundFilePath))
                {
                    foundFilePath = SearchAssemblyFileInOriginalFolders(assemblyName);
                    if (string.IsNullOrEmpty(foundFilePath))
                    {
                        if (strArrays.Length > 1)
                        {
                            if (assemblyName.EndsWith(".resources", StringComparison.CurrentCultureIgnoreCase) && !language.EndsWith("neutral", StringComparison.CurrentCultureIgnoreCase))
                            {
                                assemblyName = assemblyName.Substring(0, assemblyName.Length - ".resources".Length);
                            }
                            foundFilePath = SearchAssemblyFileInTempFolder(assemblyName);
                            if (!File.Exists(foundFilePath))
                            {
                                foundFilePath = SearchAssemblyFileInOriginalFolders(assemblyName);
                            }
                            else
                            {
                                assembly = LoadAddin(foundFilePath);
                                return assembly;
                            }
                        }
                    }
                    if (string.IsNullOrEmpty(foundFilePath))
                    {
                        OpenFileDialog openFileDialog = new OpenFileDialog();
                        openFileDialog.Filter = "Assembly files (*.dll;*.exe,*.rhp)|*.dll;*.exe;*.rhp|All files|*.*||";
                        string str = args.Name.Substring(0, args.Name.IndexOf(','));
                        openFileDialog.FileName = string.Concat(str, ".*");
                        if (openFileDialog.ShowDialog() != true)
                        {
                        }
                    }
                    assembly = CopyAndLoadAddin(foundFilePath, TempFolder,true);
                }
                else
                {
                    assembly = LoadAddin(foundFilePath);
                }
            }
            return assembly;
        }

        public void HookAssemblyResolve()
        {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
        }

        private Assembly LoadAddin(string filePath)
        {
            Assembly assembly = null;
            try
            {
                Monitor.Enter(this);
                assembly = Assembly.LoadFile(filePath);
            }
            catch(Exception ex)
            {
                throw ex;
            }
            finally
            {
                Monitor.Exit(this);
            }
            return assembly;
        }

        public Assembly LoadAddinsFromTempFolder(string originalFilePath, bool parsingOnly)
        {
            if (string.IsNullOrEmpty(originalFilePath) || originalFilePath.StartsWith("\\") || !File.Exists(originalFilePath))
            {
                throw new FileNotFoundException(originalFilePath);
            }

            StringBuilder stringBuilder = new StringBuilder(Path.GetFileNameWithoutExtension(originalFilePath));
            if (!parsingOnly)
            {
                stringBuilder.Append("-Executing-");
            }
            else
            {
                stringBuilder.Append("-Parsing-");
            }
            TempFolder = FileUtils.CreateTempFolder(stringBuilder.ToString());
            Assembly assembly = CopyAndLoadAddin(originalFilePath, TempFolder, parsingOnly);
            if (null != assembly)
            {
                return assembly;
            }
            return null;
        }

        private string SearchAssemblyFileInOriginalFolders(string assemblyName)
        {
            string[] fileExts = new string[] { ".dll", ".rhp", ".exe" };
            for (int num = 0; num < fileExts.Length; num++)
            {
                string fileExt = fileExts[num];
                string filePath = string.Concat(DotNetDir, "\\", assemblyName, fileExt);
                if (!File.Exists(filePath))
                {
                    continue;
                }
                else
                {
                    return filePath;
                }
            }
            return string.Empty;
        }

        private string SearchAssemblyFileInTempFolder(string assemblyName)
        {
            string[] fileExts = new string[] { ".dll", ".rhp", ".exe" };
            for (int i = 0; i < fileExts.Length; i++)
            {
                string fileExt = fileExts[i];
                string filePath = string.Concat(TempFolder, "\\", assemblyName, fileExt);
                if (File.Exists(filePath))
                {
                    return filePath;
                }
            }
            return string.Empty;
        }

        public void UnhookAssemblyResolve()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= new ResolveEventHandler(CurrentDomain_AssemblyResolve);
        }
    }
}
