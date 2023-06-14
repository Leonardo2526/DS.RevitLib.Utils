using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.Windows;

namespace DS.RevitLib.Utils
{
    /// <summary>
    /// Useful methods for Revit UI.
    /// </summary>
    public static class UIUtils
    {
        /// <summary>
        /// Disable <see cref="RibbonTab"/> visibility with specified <paramref name="tabName"/>.
        /// </summary>
        /// <param name="tabName"><see cref="RibbonTab"/> name to disable visibility.</param>
        public static void DisableTab(string tabName)
        {
            var tab = ComponentManager.Ribbon.Tabs.FirstOrDefault(tab => tab.Name == tabName);
            if(tab != null) { tab.IsVisible = false; }
        }

        /// <summary>
        /// Enable <see cref="RibbonTab"/> visibility with specified <paramref name="tabName"/>.
        /// </summary>
        /// <param name="tabName"><see cref="RibbonTab"/> name to enable visibility.</param>
        public static void EnableTab(string tabName)
        {
            var tab = ComponentManager.Ribbon.Tabs.FirstOrDefault(tab => tab.Name == tabName);
            if (tab != null) { tab.IsVisible = true; }
        }
    }
}
