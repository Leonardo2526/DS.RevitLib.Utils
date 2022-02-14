using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using DS.RevitUtils;
using DS.RevitUtils.MEP;


namespace DS.RevitUtils
{
    class Main
    {
        readonly Application App;
        readonly UIDocument Uidoc;
        public static Document Doc { get; set; }
        readonly UIApplication Uiapp;

        public Main(Application app, UIApplication uiapp, UIDocument uidoc, Document doc)
        {
            App = app;
            Uiapp = uiapp;
            Uidoc = uidoc;
            Doc = doc;
        }

        public static Element CurrentElement { get; set; }

        public void Implement()
        {
            PickedElement pickedElement = new PickedElement(Uidoc, Doc);
            CurrentElement = pickedElement.GetElement();

            ConnectedElement connectedElement = new ConnectedElement();
            List<Element> connectedElements = connectedElement.GetAllConnected(CurrentElement, Doc);

            TaskDialog.Show("Revit","Connected elements count: " + connectedElements.Count);
        }
    }
}
