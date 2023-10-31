using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.RevitLib.Test.TestedClasses;
using DS.RevitLib.Utils.Elements.MEPElements;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.MEP.Models;

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

            //new GetFamInstLocationTest( uidoc);
            new MEPSystemGraphFactoryTest(uidoc);

            return Autodesk.Revit.UI.Result.Succeeded;
        }
    }
}
