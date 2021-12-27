using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rhino.Toolkit.PlugInLoader
{
    public static class FileUtils
    {
        private const string TempFolderName = "RevitAddins";

        public static void CopyDirectory(string sourceDir, string destDir, List<FileInfo> allCopiedFiles)
        {
            try
            {
                string[] directories = Directory.GetDirectories(sourceDir, "*.*", SearchOption.AllDirectories);
                for (int i = 0; i < (int)directories.Length; i++)
                {
                    string sourceSubDirectionName = directories[i].Replace(sourceDir, "");
                    string destSubDirectionPath = string.Concat(destDir, sourceSubDirectionName);
                    if (!Directory.Exists(destSubDirectionPath))
                    {
                        Directory.CreateDirectory(destSubDirectionPath);
                    }
                }
                string[] files = Directory.GetFiles(sourceDir, "*.*", SearchOption.AllDirectories);
                for (int j = 0; j < (int)files.Length; j++)
                {
                    string sourceSubFileName = files[j].Replace(sourceDir, "");
                    string destSubFilePath = string.Concat(destDir, sourceSubFileName);
                    if (!Directory.Exists(Path.GetDirectoryName(destSubFilePath)))
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(destSubFilePath));
                    }
                    if (FileUtils.CopyFile(files[j], destSubFilePath))
                    {
                        allCopiedFiles.Add(new FileInfo(destSubFilePath));
                    }
                }
            }
            catch (Exception exception)
            {
            }
        }

        public static bool CopyFile(string sourceFilePath, string destFilePaht)
        {
            bool flag;
            if (!File.Exists(sourceFilePath))
            {
                return false;
            }
            File.SetAttributes(sourceFilePath, File.GetAttributes(sourceFilePath) & (FileAttributes.Hidden | FileAttributes.System | FileAttributes.Directory | FileAttributes.Archive | FileAttributes.Device | FileAttributes.Normal | FileAttributes.Temporary | FileAttributes.SparseFile | FileAttributes.ReparsePoint | FileAttributes.Compressed | FileAttributes.Offline | FileAttributes.NotContentIndexed | FileAttributes.Encrypted | FileAttributes.IntegrityStream | FileAttributes.NoScrubData));
            if (File.Exists(destFilePaht))
            {
                File.SetAttributes(destFilePaht, File.GetAttributes(destFilePaht) & (FileAttributes.Hidden | FileAttributes.System | FileAttributes.Directory | FileAttributes.Archive | FileAttributes.Device | FileAttributes.Normal | FileAttributes.Temporary | FileAttributes.SparseFile | FileAttributes.ReparsePoint | FileAttributes.Compressed | FileAttributes.Offline | FileAttributes.NotContentIndexed | FileAttributes.Encrypted | FileAttributes.IntegrityStream | FileAttributes.NoScrubData));
                File.Delete(destFilePaht);
            }
            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(destFilePaht)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(destFilePaht));
                }
                File.Copy(sourceFilePath, destFilePaht, true);
                return File.Exists(destFilePaht);
            }
            catch (Exception exception)
            {
                flag = false;
            }
            return flag;
        }

        public static string CopyFileToFolder(string sourceFilePath, string destFolder, bool onlyCopyRelated, List<FileInfo> allCopiedFiles)
        {
            string destFilePath;
            if (!File.Exists(sourceFilePath))
            {
                return null;
            }
            string directoryName = Path.GetDirectoryName(sourceFilePath);
            long folderSize = FileUtils.GetFolderSize(directoryName);
            if (folderSize <= (long)50)
            {

            }
            if (!onlyCopyRelated)
            {                
                FileUtils.CopyDirectory(directoryName, destFolder, allCopiedFiles);
            }
            else
            {
                string fileSearchPattern = string.Concat(Path.GetFileNameWithoutExtension(sourceFilePath), ".*");
                string[] files = Directory.GetFiles(directoryName, fileSearchPattern, SearchOption.TopDirectoryOnly);
                for (int i = 0; i < (int)files.Length; i++)
                {
                    string foundFileName = files[i];
                    string targetFileName = Path.Combine(destFolder, Path.GetFileName(foundFileName));
                    if (FileUtils.CopyFile(foundFileName, targetFileName))
                    {
                        allCopiedFiles.Add(new FileInfo(targetFileName));
                    }
                }
            }

            destFilePath = Path.Combine(destFolder, Path.GetFileName(sourceFilePath));
            if (File.Exists(destFilePath))
            {
                return destFilePath;
            }
            return null;
        }

        public static bool CreateFile(string filePath)
        {
            bool flag;
            if (File.Exists(filePath))
            {
                return true;
            }
            try
            {
                string directoryName = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directoryName))
                {
                    Directory.CreateDirectory(directoryName);
                }
                using (FileStream fileStream = (new FileInfo(filePath)).Create())
                {
                    FileUtils.SetWriteable(filePath);
                }
                return File.Exists(filePath);
            }
            catch (Exception exception)
            {
                flag = false;
            }
            return flag;
        }

        public static string CreateTempFolder(string prefix)
        {
            string tempPath = Path.GetTempPath();
            DirectoryInfo plugInRootDirectoryInfo = new DirectoryInfo(Path.Combine(tempPath, "RhinoPlugIns"));
            if (!plugInRootDirectoryInfo.Exists)
            {
                plugInRootDirectoryInfo.Create();
            }
            DirectoryInfo[] subDirectories = plugInRootDirectoryInfo.GetDirectories();
            for (int i = 0; i < (int)subDirectories.Length; i++)
            {
                DirectoryInfo subDirectoryInfo = subDirectories[i];
                try
                {
                    Directory.Delete(subDirectoryInfo.FullName, true);
                }
                catch (Exception exception)
                {
                }
            }
            string currentDateTimeText = string.Format("{0:yyyyMMdd_HHmmss_ffff}", DateTime.Now);
            string createdPlugInDirectPath = Path.Combine(plugInRootDirectoryInfo.FullName, string.Concat(prefix, currentDateTimeText));
            Directory.CreateDirectory(createdPlugInDirectPath);
            return createdPlugInDirectPath;
        }

        public static void DeleteFile(string fileName)
        {
            if (File.Exists(fileName))
            {
                File.SetAttributes(fileName, File.GetAttributes(fileName) & (FileAttributes.Hidden | FileAttributes.System | FileAttributes.Directory | FileAttributes.Archive | FileAttributes.Device | FileAttributes.Normal | FileAttributes.Temporary | FileAttributes.SparseFile | FileAttributes.ReparsePoint | FileAttributes.Compressed | FileAttributes.Offline | FileAttributes.NotContentIndexed | FileAttributes.Encrypted | FileAttributes.IntegrityStream | FileAttributes.NoScrubData));
                try
                {
                    File.Delete(fileName);
                }
                catch (Exception exception)
                {
                }
            }
        }

        public static bool FileExistsInFolder(string filePath, string destFolder)
        {
            string str = Path.Combine(destFolder, Path.GetFileName(filePath));
            return File.Exists(str);
        }

        public static long GetFolderSize(string folderPath)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);
            long folderSize = (long)0;
            FileSystemInfo[] fileSystemInfos = directoryInfo.GetFileSystemInfos();
            for (int i = 0; i < (int)fileSystemInfos.Length; i++)
            {
                FileSystemInfo fileSystemInfo = fileSystemInfos[i];
                if (!(fileSystemInfo is FileInfo))
                {
                    folderSize += FileUtils.GetFolderSize(fileSystemInfo.FullName);
                }
                else
                {
                    folderSize += ((FileInfo)fileSystemInfo).Length;
                }
            }
            return folderSize / (long)1024 / (long)1024;
        }

        public static DateTime GetModifyTime(string filePath)
        {
            return File.GetLastWriteTime(filePath);
        }

        public static bool SameFile(string file1, string file2)
        {
            return 0 == string.Compare(file1.Trim(), file2.Trim(), true);
        }

        public static void SetWriteable(string fileName)
        {
            if (File.Exists(fileName))
            {
                File.SetAttributes(fileName, File.GetAttributes(fileName) & (FileAttributes.Hidden | FileAttributes.System | FileAttributes.Directory | FileAttributes.Archive | FileAttributes.Device | FileAttributes.Normal | FileAttributes.Temporary | FileAttributes.SparseFile | FileAttributes.ReparsePoint | FileAttributes.Compressed | FileAttributes.Offline | FileAttributes.NotContentIndexed | FileAttributes.Encrypted | FileAttributes.IntegrityStream | FileAttributes.NoScrubData));
            }
        }
    }
}
