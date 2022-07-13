using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using Rhino.Toolkit.PlugInUninstaller.ViewModel;
using System;
using System.Collections.Generic;

namespace Rhino.Toolkit.PlugInUninstaller
{
    public class PlugInUninstallerCommand : Command
    {
        public PlugInUninstallerCommand()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }

        ///<summary>The only instance of this command.</summary>
        public static PlugInUninstallerCommand Instance { get; private set; }

        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName => "PlugInUninstaller";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            PlugInUninstallerVM plugInUninstallerVM = new PlugInUninstallerVM();
            MainWindow mainWindow = new MainWindow();
            mainWindow.DataContext = plugInUninstallerVM;
            mainWindow.ShowDialog();
            return Result.Success;
        }
    }
}
