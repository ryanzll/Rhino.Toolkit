using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rhino.Toolkit.PlugInLoader
{
    public class PlugIn
    {
        public string PlugInAssemblyName { get; set; }

        public string PlugInAssemblyPath { get; set; }

        public IList<PlugInCommand> PlugInCommands { get; set; } = new List<PlugInCommand>();
    }

    public class PlugInCommand
    {
        public string PlugInAssemblyPath { get; set; }

        public string FullName { get; set; }

        public string Name { get; set; }
    }
}
