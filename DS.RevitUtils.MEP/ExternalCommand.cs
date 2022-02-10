using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;

namespace DS.RevitUtils.MEP
{
    [Transaction(TransactionMode.Manual)]
    class ExternalCommand : IExternalCommand
    {
        public Autodesk.Revit.UI.Result Execute(ExternalCommandData commandData,
           ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            Autodesk.Revit.ApplicationServices.Application application = uiapp.Application;

            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uiapp.ActiveUIDocument.Document;

            List<XYZ> points = new List<XYZ>()
            {
                new XYZ(0,0,0),
                new XYZ(3,0,0),
                new XYZ(3,3,0),
                new XYZ(6,3,0),
                new XYZ(6,0,0),
                new XYZ(9,0,0)
            };

            // Find collisions between elements and a selected element
            Reference reference = uidoc.Selection.PickObject(ObjectType.Element, "Select element that will be checked for intersection with all elements");
            Element elementA = doc.GetElement(reference);

            PypeSystem pypeSystem = new PypeSystem(uiapp, uidoc, doc, elementA);
            pypeSystem.CreatePipeSystem(points);

            //DSPipe pipe = new DSPipe(uiapp, uidoc, doc);
            //pipe.CreatePipeSystem();

            ////pipe.DeleteElement();
            //pipe.SplitElement(); 


            TaskDialog.Show("Revit", "Pipe created!");
            return Autodesk.Revit.UI.Result.Succeeded;
        }
    }
}