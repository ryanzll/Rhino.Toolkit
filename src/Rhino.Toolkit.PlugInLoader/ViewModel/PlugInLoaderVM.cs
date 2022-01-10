using Microsoft.Win32;
using Rhino.Commands;
using Rhino.Toolkit.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Rhino.Toolkit.PlugInLoader.ViewModel
{
    public class PlugInLoaderVM: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void Notify(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private ObservableCollection<TreeViewItemVM> _plugInTreeView;

        public ObservableCollection<TreeViewItemVM> PlugInTreeView
        {
            get { return _plugInTreeView; }
            private set
            {
                _plugInTreeView = value;
                Notify(nameof(PlugInTreeView));
            }
        }

        private TreeViewItemVM _selectedPlugInTreeViewItem;
        public TreeViewItemVM SelectedPlugInTreeViewItem 
        {
            get { return _selectedPlugInTreeViewItem; }
            set
            {
                _selectedPlugInTreeViewItem = value;
                Notify(nameof(SelectedPlugInTreeViewItem));
            }
        }

        PlugInLoader PlugInLoader { get; set; }

        public ICommand LoadCommand { get; private set; }

        public ICommand RunCommand { get; private set; }

        public ICommand RemoveCommand { get; private set; }

        public ICommand SelectedItemChangedCommand { get; private set; }

        public Result Result { get; private set; } = Result.Nothing;

        public RhinoDoc Doc { get; set; }

        public RunMode Mode { get; set; }

        public PlugInLoaderVM(RhinoDoc doc, RunMode mode)
        {
            Doc = doc;
            Mode = mode;
            PlugInLoader = new PlugInLoader();
            PlugInTreeView = new ObservableCollection<TreeViewItemVM>();
            if(null != PlugInLoader.PlugIns)
            {
                foreach (PlugIn plugIn in PlugInLoader.PlugIns)
                {
                    TreeViewItemVM treeViewItem = new PlugInTreeViewItem(plugIn);
                    PlugInTreeView.Add(treeViewItem);
                }
            }
            LoadCommand = new RelayCommand(() => { return true; }, LoadPlugIn);
            RunCommand = new RelayCommand<Window>((window) => { return true; }, (window) => RunPlugIn(window));
            RemoveCommand = new RelayCommand(() => { return true; }, RemovePlugIn);
            SelectedItemChangedCommand = new RelayCommand<object>((obj) => { return true; }, SelectedItemChanged);
        }

        protected void LoadPlugIn()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Assembly files (*.dll;*.exe,*.rhp)|*.dll;*.exe;*.rhp|All files|*.*||";
            var result = openFileDialog.ShowDialog();
            if (!result.HasValue || !result.Value)
            {
                return;
            }
            PlugIn plugIn = PlugInLoader.Load(openFileDialog.FileName);
            if(null == plugIn)
            {
                return;
            }
            TreeViewItemVM treeViewItem = new PlugInTreeViewItem(plugIn);
            PlugInTreeView.Add(treeViewItem);
        }

        protected void RunPlugIn(Window window)
        {
            if (SelectedPlugInTreeViewItem is PlugInTreeViewItem)
            {
                return;
            }

            window.Hide();
            PlugInCommandTreeViewItem plugInCommandTreeViewItem = SelectedPlugInTreeViewItem as PlugInCommandTreeViewItem;
            try
            {
                Result = PlugInLoader.RunCommand(Doc, Mode, plugInCommandTreeViewItem.PlugInCommand);
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                window.Show();
                return;
            }
            window.Close();
        }

        protected void RemovePlugIn()
        {
            if(SelectedPlugInTreeViewItem is PlugInCommandTreeViewItem)
            {
                return;
            }
            PlugInTreeViewItem plugInTreeViewItem = SelectedPlugInTreeViewItem as PlugInTreeViewItem;
            PlugInTreeView.Remove(SelectedPlugInTreeViewItem);
            PlugInLoader.RemovePlugIn(plugInTreeViewItem.PlugIn);
        }

        protected void SelectedItemChanged(object obj)
        {
            SelectedPlugInTreeViewItem = obj as TreeViewItemVM;
        }
    }
}
