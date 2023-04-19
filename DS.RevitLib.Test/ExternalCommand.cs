using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using DS.RevitLib.Test.TestedClasses;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Various;
using DS.RevitLib.Utils.Extensions;
using Autodesk.Revit.UI.Selection;

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

            new GetElementsIntersectionTest(uidoc);

            return Autodesk.Revit.UI.Result.Succeeded;
        }
    }
}
