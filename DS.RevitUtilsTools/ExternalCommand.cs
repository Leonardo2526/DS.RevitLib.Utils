using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;

namespace DS.RevitUtils.GPExtractor
{
    [Transaction(TransactionMode.Manual)]
    public class ExternalCommand : IExternalCommand
    {
        public Autodesk.Revit.UI.Result Execute(ExternalCommandData commandData,
           ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            Autodesk.Revit.ApplicationServices.Application app = uiapp.Application;

            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uiapp.ActiveUIDocument.Document;

            GPCreator gPCreator = new GPCreator(app, uiapp, uidoc, doc);
            gPCreator.Create();

            return Autodesk.Revit.UI.Result.Succeeded;
        }
    }
}