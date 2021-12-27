using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rhino.Toolkit.PlugInLoader.ViewModel
{
    public class TreeViewItemVM
    {
        public string Name { get; set; }

        public string FilePath { get; set; }

        public bool IsChecked { get; set; }

        public bool IsExpanded { get; set; } = true;

        public ObservableCollection<TreeViewItemVM> Children { get; set; }
    }

    public class PlugInTreeViewItem: TreeViewItemVM
    {
        public PlugIn PlugIn { get; set; }

        public PlugInTreeViewItem(PlugIn plugIn)
        {
            if(null == plugIn)
            {
                return;
            }
            PlugIn = plugIn;
            Name = plugIn.PlugInAssemblyName;
            FilePath = plugIn.PlugInAssemblyPath;

            Children = new ObservableCollection<TreeViewItemVM>();
            foreach (PlugInCommand plugInCommand in plugIn.PlugInCommands)
            {
                PlugInCommandTreeViewItem plugInCommandTreeViewItem = new PlugInCommandTreeViewItem(plugInCommand);
                Children.Add(plugInCommandTreeViewItem);
            }
        }
    }

    public class PlugInCommandTreeViewItem : TreeViewItemVM
    {
        public PlugInCommand PlugInCommand { get; set; }

        public PlugInCommandTreeViewItem(PlugInCommand plugInCommand)
        {
            PlugInCommand = plugInCommand;
            Name = plugInCommand.FullName;
            FilePath = plugInCommand.PlugInAssemblyPath;
        }
    }
}
