using Microsoft.Win32;
using Rhino.PlugIns;
using Rhino.Toolkit.PlugInUninstaller.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rhino.Toolkit.PlugInUninstaller
{
    public class PlugInUninstaller
    {
        const string HKEY_CURRENT_USER = @"\\HKEY_CURRENT_USER\";

        const string HKEY_LOCAL_MACHINE = @"\\HKEY_LOCAL_MACHINE\";

        public void Uninstall(PlugInInfoVM plugInInfoVM)
        {
            string registryKeyPath = plugInInfoVM.RegistryPath;
            if(string.IsNullOrEmpty(registryKeyPath))
            {
                return;
            }

            RegistryKey rootKey = null;
            string subKeyName = null;
            if (registryKeyPath.StartsWith(HKEY_CURRENT_USER))
            {
                rootKey = Registry.CurrentUser;
                subKeyName = registryKeyPath.Replace(HKEY_CURRENT_USER, "");
            }
            else if(registryKeyPath.StartsWith(HKEY_LOCAL_MACHINE))
            {
                rootKey = Registry.LocalMachine;
                subKeyName = registryKeyPath.Replace(HKEY_CURRENT_USER, "");
            }
            
            rootKey.DeleteSubKeyTree(subKeyName);
            rootKey.Close();
        }

        public ObservableCollection<PlugInInfoVM> GetInstalledPlugInInfoVMs()
        {
            ObservableCollection<PlugInInfoVM> plugInInfoVMs = new ObservableCollection<PlugInInfoVM>();
            Dictionary<Guid, string>  installedPlugIns = PlugIn.GetInstalledPlugIns();
            foreach(var plugInGuidName in installedPlugIns)
            {
                PlugInInfo plugInInfo = PlugIn.GetPlugInInfo(plugInGuidName.Key);
                PlugInInfoVM plugInInfoVM = new PlugInInfoVM(plugInInfo);
                plugInInfoVMs.Add(plugInInfoVM);
            }
            return plugInInfoVMs;
        }
    }
}
