using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DS.RevitUtils.Ribbon
{
    public class ExternalApplication : IExternalApplication
    {
        // class instance
        internal static ExternalApplication thisApp = null;

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            Button button = new Button();
            button.AddButtons();

            // Create a custom ribbon tab 
            string tabName = "DS.RevitUtils";
            application.CreateRibbonTab(tabName);

            // Create a ribbon panel
            RibbonPanel m_projectPanel_1 = application.CreateRibbonPanel(tabName, "Tools");


            // Add the button to the panel
            RibbonItem ribbonItem = m_projectPanel_1.AddItem(Button.button1);
            List<RibbonItem> projectButtons = new List<RibbonItem>
            {
                ribbonItem
            };

            // Add the buttons to the panels
            //projectButtons.AddRange(m_projectPanel_1.AddStackedItems(button1, button2));

            return Result.Succeeded;
        }
    }
}
