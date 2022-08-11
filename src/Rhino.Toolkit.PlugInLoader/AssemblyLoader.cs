using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Rhino.Toolkit.PlugInLoader
{
    public class AssemblyLoader
    {
        private List<string> OriginalFolders { get; set; }

        private static string DotNetDir { get; set; }

        public string TempFolder{get;set;}

        public bool LoadAssemblyManually { get; set; }

        static List<string> DefaultLanguageFolderNames { get; set; } = new List<string>();

        static AssemblyLoader()
        {
            DotNetDir = string.Concat(Environment.GetEnvironmentVariable("windir"), "\\Microsoft.NET\\Framework\\v2.0.50727");
            DefaultLanguageFolderNames.AddRange(new string[] { "cn", "es", "en" });
        }

        public AssemblyLoader()
        {
            TempFolder = string.Empty;
            OriginalFolders = new List<string>();
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            Assembly assembly;
            lock (this)
            {
                string[] strArrays = args.Name.Split(new char[] { ',' });

                string assemblyName = strArrays[0];
                string language = strArrays[2];
                //can't parse correctly, just return null
                if (strArrays.Length <= 1)
                {
                    return null;
                }

                string foundFilePath = SearchAssemblyInFolder(TempFolder, assemblyName);
                if (!File.Exists(foundFilePath))
                {
                    foundFilePath = SearchAssemblyInFolder(DotNetDir, assemblyName);
                    if (string.IsNullOrEmpty(foundFilePath))
                    {
                        foundFilePath = SearchAssemblyInFolder(TempFolder, assemblyName);
                        if (!File.Exists(foundFilePath))
                        {
                            foundFilePath = SearchAssemblyInFolder(DotNetDir, assemblyName);
                        }
                        else
                        {
                            assembly = LoadAddin(foundFilePath);
                            return assembly;
                        }
                    }
                    if ( string.IsNullOrEmpty(foundFilePath))
                    {
                        if(LoadAssemblyManually)
                        {
                            OpenFileDialog openFileDialog = new OpenFileDialog();
                            openFileDialog.Filter = "Assembly files (*.dll;*.exe,*.rhp)|*.dll;*.exe;*.rhp|All files|*.*||";
                            string str = args.Name.Substring(0, args.Name.IndexOf(','));
                            openFileDialog.FileName = string.Concat(str, ".*");
                            if (openFileDialog.ShowDialog() != true)
                            {
                                return null;
                            }
                        }
                        else
                        {
                            return null;
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

        private string SearchAssemblyInFolder(string folderPath, string assemblyName, bool searchSubFolder = true)
        {
            string[] fileExts = new string[] { ".dll", ".rhp", ".exe" };
            for (int num = 0; num < fileExts.Length; num++)
            {
                string fileExt = fileExts[num];
                string filePath = string.Concat(folderPath, "\\", assemblyName, fileExt);
                if (!File.Exists(filePath))
                {
                    continue;
                }
                else
                {
                    return filePath;
                }
            }

            if (assemblyName.EndsWith(".resources", StringComparison.CurrentCultureIgnoreCase))
            {
                foreach(var languageFolderName in DefaultLanguageFolderNames)
                {
                    string languageFolderPath = Path.Combine(folderPath, languageFolderName);
                    if(Directory.Exists(languageFolderPath))
                    {
                        string filePath = Path.Combine(languageFolderPath, assemblyName);
                        if(File.Exists(filePath))
                        {
                            return filePath;
                        }
                    }
                }
            }

            if(!searchSubFolder)
            {
                return string.Empty;
            }
            DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);
            var subFolders = directoryInfo.GetDirectories();
            foreach(var subFolder in subFolders)
            {
                string filePath = SearchAssemblyInFolder(subFolder.FullName, assemblyName, false);
                if(string.IsNullOrEmpty(filePath))
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
