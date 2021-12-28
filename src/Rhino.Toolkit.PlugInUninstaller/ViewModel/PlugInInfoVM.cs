using Rhino.PlugIns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rhino.Toolkit.PlugInUninstaller.ViewModel
{
    public class PlugInInfoVM
    {
        public string Name { get; set; }

        public string AssemblyPath { get; set; }

        public string RegistryPath { get; set; }

        public string CommandsText
        {
            get
            {
                var commandNames = PlugInInfo.CommandNames;
                if(null == commandNames || !commandNames.Any())
                {
                    return null;
                }
                StringBuilder sb = new StringBuilder();
                for(int index = 0; index < commandNames.Length; index++)
                {
                    if (index == commandNames.Length - 1)
                    {
                        sb.Append(commandNames[index]);
                    }
                    else
                    {
                        sb.AppendLine(commandNames[index]);
                    }
                }
                return sb.ToString();
            }
        }

        PlugInInfo PlugInInfo { get; set; }

        public PlugInInfoVM(PlugInInfo plugInInfo)
        {
            PlugInInfo = plugInInfo;
            Name = plugInInfo.Name;
            RegistryPath = plugInInfo.RegistryPath;
            AssemblyPath = plugInInfo.FileName;
        }
    }
}
