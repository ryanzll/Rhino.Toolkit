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
                AssemblyName assemblyName = new AssemblyName(args.Name);
                string foundFilePath = SearchAssemblyFileInTempFolder(args.Name);
                if (!File.Exists(foundFilePath))
                {
                    foundFilePath = SearchAssemblyFileInOriginalFolders(args.Name);
                    if (string.IsNullOrEmpty(foundFilePath))
                    {
                        string[] strArrays = args.Name.Split(new char[] { ',' });
                        string str = strArrays[0];
                        if ((int)strArrays.Length > 1)
                        {
                            string str1 = strArrays[2];
                            if (str.EndsWith(".resources", StringComparison.CurrentCultureIgnoreCase) && !str1.EndsWith("neutral", StringComparison.CurrentCultureIgnoreCase))
                            {
                                str = str.Substring(0, str.Length - ".resources".Length);
                            }
                            foundFilePath = SearchAssemblyFileInTempFolder(str);
                            if (!File.Exists(foundFilePath))
                            {
                                foundFilePath = SearchAssemblyFileInOriginalFolders(str);
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
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(this.CurrentDomain_AssemblyResolve);
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
            string str;
            string[] strArrays = new string[] { ".dll", ".exe" };
            string filePath = string.Empty;
            string str1 = assemblyName.Substring(0, assemblyName.IndexOf(','));
            int num = 0;
            while (true)
            {
                if (num < (int)strArrays.Length)
                {
                    string str2 = strArrays[num];
                    filePath = string.Concat(DotNetDir, "\\", str1, str2);
                    if (!File.Exists(filePath))
                    {
                        num++;
                    }
                    else
                    {
                        str = filePath;
                        break;
                    }
                }
                else
                {
                    for (int i = 0; i < (int)strArrays.Length; i++)
                    {
                        string str3 = strArrays[i];
                        foreach (string originalFolder in this.OriginalFolders)
                        {
                            filePath = string.Concat(originalFolder, "\\", str1, str3);
                            if (!File.Exists(filePath))
                            {
                                continue;
                            }
                            str = filePath;
                            return str;
                        }
                    }                                     
                    return SearchAssemblyFileInOriginalFolders(assemblyName);
                }
            }
            return str;
        }

        private string SearchAssemblyFileInTempFolder(string assemblyName)
        {
            string[] strArrays = new string[] { ".dll", ".exe" };
            string filePath = string.Empty;
            string str = assemblyName.Substring(0, assemblyName.IndexOf(','));
            for (int i = 0; i < (int)strArrays.Length; i++)
            {
                string str1 = strArrays[i];
                filePath = string.Concat(TempFolder, "\\", str, str1);
                if (File.Exists(filePath))
                {
                    return filePath;
                }
            }
            return string.Empty;
        }

        public void UnhookAssemblyResolve()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= new ResolveEventHandler(this.CurrentDomain_AssemblyResolve);
        }
    }
}
