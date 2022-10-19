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
            Document doc = uiapp.ActiveUIDocument.Document;

            //var test = new AlignMEPCurvesTest(uidoc);
            //var test = new BuilderByPointsTest(uidoc);
            var test = new GetAssociatedParameterTest(uidoc);
            test.Run();

            //var client = new ConnectionFactoryClient(uidoc);
            //client.Run();


            //var pathFinderTest = new SimplePathFinderTest(uidoc, doc);
            //var path = pathFinderTest.RunTest2();
            //pathFinderTest.ShowPath(path);
            //SolidCollisionCheckerTest.RunWithLink(doc);

            //var selector = new SystemModelTest(uidoc, doc, uiapp);
            //selector.RunTest();

            //var test = new SolidPlacerTest(uidoc, doc, uiapp);
            //test.Run();

            return Autodesk.Revit.UI.Result.Succeeded;
        }
    }
}
