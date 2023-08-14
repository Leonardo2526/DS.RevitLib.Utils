using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.RevitLib.Test.TestedClasses;

namespace DS.RevitLib.Test
{
    [Transaction(TransactionMode.Manual)]
    public class ExternalCommand : IExternalCommand
    {
        public Autodesk.Revit.UI.Result Execute(ExternalCommandData commandData,
           ref string message, ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            Autodesk.Revit.ApplicationServices.Application application = uiapp.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;

            new AlignMEPCurvesTest( uidoc);

            return Autodesk.Revit.UI.Result.Succeeded;
        }
    }
}
