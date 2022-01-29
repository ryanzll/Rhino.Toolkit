using Rhino.Toolkit.Common.MVVM;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace Rhino.Toolkit.PlugInUninstaller.ViewModel
{
    public class PlugInUninstallerVM: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void Notify(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ObservableCollection<PlugInInfoVM> PlugInInfoVMs { get; set; }

        PlugInInfoVM _selectedPlugInInfoVM;

        public PlugInInfoVM SelectedPlugInInfoVM
        {
            get { return _selectedPlugInInfoVM; } 
            set
            {
                _selectedPlugInInfoVM = value;
                Notify(nameof(SelectedPlugInInfoVM));
            }
        }

        public ICommand UninstallCommand { get; set; }

        PlugInUninstaller PlugInUninstaller { get; set; }

        string CurrentRhinoInsallDirection { get; set; }

        public PlugInUninstallerVM()
        {
            PlugInUninstaller = new PlugInUninstaller();
            PlugInInfoVMs = PlugInUninstaller.GetInstalledPlugInInfoVMs();
            UninstallCommand = new RelayCommand(() => { return true; }, Uninstall);
            var process = Process.GetCurrentProcess(); // Or whatever method you are using
            string rhinoExePath = process.MainModule.FileName;
            CurrentRhinoInsallDirection = Path.GetDirectoryName(Path.GetDirectoryName(rhinoExePath));
        }

        private void Uninstall()
        {
            if(null == SelectedPlugInInfoVM)
            {
                return;
            }

            if(SelectedPlugInInfoVM.AssemblyPath.Contains(CurrentRhinoInsallDirection))
            {
                MessageBox.Show("Current PlugIn is System builtin, please not uninsall for safety");
                return;
            }
            PlugInUninstaller.Uninstall(SelectedPlugInInfoVM);
            PlugInInfoVMs.Remove(SelectedPlugInInfoVM);
        }
    }
}
