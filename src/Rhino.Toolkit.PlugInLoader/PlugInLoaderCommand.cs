using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Input;
using Rhino.Input.Custom;
using Rhino.Toolkit.PlugInLoader.ViewModel;
using System;
using System.Collections.Generic;

namespace Rhino.Toolkit.PlugInLoader
{
    public class PlugInLoaderCommand : Command
    {
        public PlugInLoaderCommand()
        {
            // Rhino only creates one instance of each command class defined in a
            // plug-in, so it is safe to store a refence in a static property.
            Instance = this;
        }

        ///<summary>The only instance of this command.</summary>
        public static PlugInLoaderCommand Instance { get; private set; }

        ///<returns>The command name as it appears on the Rhino command line.</returns>
        public override string EnglishName => "PlugInLoader";

        protected override Result RunCommand(RhinoDoc doc, RunMode mode)
        {
            PlugInLoaderVM plugInLoaderVM = new PlugInLoaderVM(doc,mode);
            MainWindow mainWindow = new MainWindow();
            mainWindow.DataContext = plugInLoaderVM;
            var result = mainWindow.ShowDialog();
            if (!result.HasValue || !result.Value)
            {
                return Result.Cancel;
            }

            return plugInLoaderVM.Result;
        }
    }
}
