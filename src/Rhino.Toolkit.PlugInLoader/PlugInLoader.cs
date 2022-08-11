﻿using Rhino.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Rhino.Toolkit.PlugInLoader
{
    public class PlugInLoader
    {
        public Settings Settings { get; set; } = new Settings();

        public IList<PlugIn> PlugIns { get; set; }

        public bool LoadAssemblyManually { get; set; }

        public PlugInLoader()
        {
            PlugIns = Settings.Load();
            if (null == PlugIns)
            {
                PlugIns = new List<PlugIn>();
            }
        }

        public PlugIn Load(string assemblyPath)
        {
            if (PlugIns.Where(p => 0 == string.Compare(assemblyPath, p.PlugInAssemblyPath, StringComparison.OrdinalIgnoreCase)).Any())
            {
                MessageBox.Show("Selected plugin already existed");
                return null;
            }
            if (string.IsNullOrEmpty(assemblyPath) || !File.Exists(assemblyPath))
            {
                return null;
            }

            PlugIn plugIn = new PlugIn()
            {
                PlugInAssemblyPath = assemblyPath,
                PlugInAssemblyName = Path.GetFileName(assemblyPath),
            };
            AssemblyLoader assemblyLoader = new AssemblyLoader() { LoadAssemblyManually = LoadAssemblyManually};
            Assembly assembly = assemblyLoader.LoadAddinsFromTempFolder(assemblyPath, true);
            Type commandBaseType = typeof(Command);
            IEnumerable<Type> commandTypes = assembly.GetTypes().Where(t => t.IsClass && commandBaseType.IsAssignableFrom(t));
            foreach (Type commandType in commandTypes)
            {
                MethodInfo runCommandInfo = commandType.GetMethod("RunCommand", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (null == runCommandInfo)
                {
                    continue;
                }
                PlugInCommand plugInCommand = new PlugInCommand()
                {
                    Name = commandType.Name,
                    FullName = commandType.FullName,
                    PlugInAssemblyPath = assemblyPath,
                };
                plugIn.PlugInCommands.Add(plugInCommand);
            }

            PlugIns.Add(plugIn);
            Settings.Save(PlugIns);
            return plugIn;
        }

        public void RemovePlugIn(PlugIn plugIn)
        {
            PlugIns.Remove(plugIn);
            Settings.Save(PlugIns);
        }

        public Result RunCommand(RhinoDoc doc, RunMode mode, PlugInCommand plugInCommand)
        {
            if (null == plugInCommand)
            {
                throw new ArgumentNullException("plugInCommand");
            }
            AssemblyLoader assemblyLoader = new AssemblyLoader() { LoadAssemblyManually = LoadAssemblyManually };
            assemblyLoader.HookAssemblyResolve();

            try
            {
                Assembly assembly = assemblyLoader.LoadAddinsFromTempFolder(plugInCommand.PlugInAssemblyPath, false);
                Command command = assembly.CreateInstance(plugInCommand.FullName) as Command;
                if (null == command)
                {
                    throw new ArgumentException(plugInCommand.FullName);
                }
                Type type = command.GetType();
                MethodInfo runCommandInfo = type.GetMethod("RunCommand", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                object result = runCommandInfo.Invoke(command, new object[] { doc, mode });
                if (null == result)
                {
                    return Result.Failure;
                }
                return (Result)result;
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                return Result.Failure;
            }
            finally
            {
                assemblyLoader.UnhookAssemblyResolve();
            }            
        }
    }
}
