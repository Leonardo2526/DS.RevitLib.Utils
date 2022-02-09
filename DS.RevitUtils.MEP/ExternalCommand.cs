using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
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

            PypeSystem pypeSystem = new PypeSystem(uiapp, uidoc, doc);
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