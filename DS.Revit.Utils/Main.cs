using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using DS.Revit.Utils;
using DS.Revit.Utils.MEP;


namespace DS.Revit.Utils
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

            List<Element> connectedElements = ConnectedElement.GetAllConnected(CurrentElement, Doc);

            TaskDialog.Show("Revit","Connected elements count: " + connectedElements.Count);
        }
    }
}
